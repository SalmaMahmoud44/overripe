using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float jumpSpeed = 15f;
    [SerializeField] float footstepInterval = 0.5f;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.15f;
    [SerializeField] LayerMask groundLayer;

    [Header("Jump Feel")]
    [SerializeField] float coyoteTime = 0.12f;
    [SerializeField] float jumpBufferTime = 0.12f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float dashCooldown = 1f;

    [Header("Shoot Settings")]
    [SerializeField] ArrowProjectille arrowPrefab;
    [SerializeField] Transform arrowSpawnPoint;
    [SerializeField] float arrowCooldown = 0.5f;
    [SerializeField] LevelManager levelManager;

    [Header("Melee Settings")]
    [SerializeField] float meleeCooldown = 1f;
    [SerializeField] Transform meleeSpawnPoint;
    [SerializeField] float meleeRange = 1f;
    [SerializeField] float meleeDamage = 10f;
    [SerializeField] LayerMask enemyLayer ;

    [Header("Laser Settings")]
    [SerializeField] TripleBLaser tripleBLaserPrefab;
    [SerializeField] float maxLaserAngle = 45f;


    public bool isFacingRight = true;


    public event Action<KeyCode> OnPlayerMoved;
    public event Action OnPlayerJumped;
    public event Action OnPlayerDashed;
    public event Action OnPlayerMelee;


    float shootTimer = 0f;  
    float meleeTimer = 0f;
    float footstepTimer = 0f;

    bool isDashing = false;
    bool canDash = true;
    bool controlsLocked = false;
    bool nextMeleeFirst = true;

    Vector2 moveInput;

    float coyoteTimeCounter;
    float jumpBufferCounter;
    bool isGrounded;


    Rigidbody2D myRigidbody;
    Transform myTransform;
    CapsuleCollider2D myCollider;
    Animator myAnimator;
    AudioManager playerAudio;
    KnockBack knockBack;

    Vector2 worldPos;
    Vector2 mousePos;
    RaycastHit2D[] hits;


    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myTransform = GetComponent<Transform>();
        myCollider = GetComponent<CapsuleCollider2D>();
        enemyLayer = LayerMask.GetMask("Enemy");
        myAnimator = GetComponentInChildren<Animator>();
        playerAudio = GetComponent<AudioManager>();
        knockBack = GetComponent<KnockBack>();

        arrowSpawnPoint = transform.Find("ArrowSpawnPoint");
        meleeSpawnPoint = transform.Find("MeleeSpawnPoint");

        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");

        if (enemyLayer == 0)
            enemyLayer = LayerMask.GetMask("Enemy");

        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();

        if(tripleBLaserPrefab == null)
            tripleBLaserPrefab = GameObject.Find("TripleB").GetComponent<TripleBLaser>();
    }

    void Update()
    {
        UpdateTimers();

        CheckGround();

        UpdateJumpTimers();

        if (IsKnockedBack())
        {
            UpdateAnimation();
            return;
        }

        if (!controlsLocked && !isDashing)
        {
            Run();
            Flip();
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (isDashing)
            return;

        if (myRigidbody.linearVelocity.y < 0f)
        {
            myRigidbody.linearVelocity +=Vector2.up *Physics2D.gravity.y *(fallMultiplier - 1f) *Time.fixedDeltaTime;
        }
        else if (myRigidbody.linearVelocity.y > 0f &&!Keyboard.current.spaceKey.isPressed)
        {
            myRigidbody.linearVelocity += Vector2.up * Physics2D.gravity.y *(lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    void UpdateTimers()
    {
        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime;

        if (meleeTimer > 0f)
            meleeTimer -= Time.deltaTime;

        if (footstepTimer > 0f)
            footstepTimer -= Time.deltaTime;
    }
      void OnMove(InputValue value)
      {
           moveInput = value.Get<Vector2>();

           if (controlsLocked)
           {
               moveInput = Vector2.zero; 
               return;
           }
           if (moveInput.x != 0f)
           {
               KeyCode keyPressed = moveInput.x > 0f ? KeyCode.D : KeyCode.A;
               OnPlayerMoved?.Invoke(keyPressed); 
           }     
      }
    void OnJump(InputValue value)
    {
        if (IsKnockedBack())
            return;

        if (isDashing)
            return; 

        if (controlsLocked)
        {
            return;
        }

        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }
    void OnDash(InputValue value)
    {
        if (IsKnockedBack())
            return;

        if (!canDash)
            return; 

        if (controlsLocked)
        {
            return;
        }


        if (value.isPressed && !isDashing)
        {
            StartCoroutine(Dash());
            OnPlayerDashed?.Invoke(); 
        }
    }

    void OnShoot(InputValue value)
    {
        if (value.isPressed && (levelManager.curreLevel == "Peach" || levelManager.currentLevelIndex == 4) )
        {
            ShootArrow();
        }
    }
    void OnMelee(InputValue value)
    {
        if (IsKnockedBack())
            return;

        if (!value.isPressed)
            return; 

        if (controlsLocked)
        {
            return;
        }

        if (MeleeAttack())
        {
            OnPlayerMelee?.Invoke(); 

        }
    }

    void OnLaser(InputValue value)
    {

        Debug.Log("Laser input received. Current Level: " + levelManager.curreLevel + ", Level Index: " + levelManager.currentLevelIndex);
        if (controlsLocked)
            return;

        if (value.isPressed)
        {
            if (tripleBLaserPrefab != null)
            {
                worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                mousePos = new Vector2(worldPos.x, worldPos.y);
                Vector2 shootDirection = (mousePos - (Vector2)tripleBLaserPrefab.transform.position);

                if (shootDirection.sqrMagnitude <= 0.001f)
                    return;

                shootDirection.Normalize();

                float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;

                float facingAngle = isFacingRight ? 0f : 180f;

                float angleDifferance = Mathf.DeltaAngle(facingAngle,angle);

                if (Mathf.Abs(angleDifferance) > maxLaserAngle)
                {
                    Debug.Log("Laser blocked Angle: " + angleDifferance);
                    return;
                }
                  

                tripleBLaserPrefab.ShootLaser(shootDirection);


            }
        }
    }
    public void SetControlsLocked(bool locked)
    {
        controlsLocked = locked;

        if (locked)
        {
            moveInput = Vector2.zero;

            if (myRigidbody != null)
            {
                myRigidbody.linearVelocity = new Vector2(0f,myRigidbody.linearVelocity.y);
            }
            myAnimator.SetBool("isRunning", false);
        }
    }
    void Run()
    {
        if(isDashing) 
            return; 

        Vector2 playerVelocity = new Vector2(moveInput.x * moveSpeed, myRigidbody.linearVelocity.y);
        myRigidbody.linearVelocity = playerVelocity;

        bool isMoving = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", isMoving);

        if(isMoving && isGrounded)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.footstepClip);
                footstepTimer = 0f; 
            }
        }
        else if (!isMoving)
        {
            footstepTimer = 0f;
        }
    }

    void Flip()
    {
        bool isMoving = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        if (isMoving) 
          myTransform.localScale = new Vector2(Mathf.Sign(myRigidbody.linearVelocity.x), 1f);
        if(myTransform.localScale.x > 0f)
            isFacingRight = true;
        else
            isFacingRight = false;  
    }

    void CheckGround()
    {
        if(groundCheck == null)
        {
            isGrounded =myCollider != null && myCollider.IsTouchingLayers(groundLayer);

            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position,groundCheckRadius,groundLayer );
    }

    void UpdateJumpTimers()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f &&coyoteTimeCounter > 0f &&!isDashing &&!controlsLocked)
        {
            PerformJump();
        }
    }
    void PerformJump()
    {
        myRigidbody.linearVelocity = new Vector2(myRigidbody.linearVelocity.x,jumpSpeed);

        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;

        if (AudioManager.Instance != null)
        {AudioManager.Instance.PlaySFX( AudioManager.Instance.jumpClip);
        }

        OnPlayerJumped?.Invoke();
    }
    void ShootArrow()
    {

        if (arrowPrefab == null || arrowSpawnPoint == null)
        {
            Debug.LogWarning("Arrow prefab or spawn point is not assigned.");
            return;
        }
        if (shootTimer > 0f)
            return; 

        shootTimer = arrowCooldown; 


        worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); 
        mousePos = new Vector2(worldPos.x, worldPos.y); 
        Vector2 shootDirection = (mousePos - (Vector2)arrowSpawnPoint.position).normalized; 
        ArrowProjectille arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        arrow.Init(shootDirection);
    }   

      bool MeleeAttack()
      {
            if (meleeTimer > 0f)
                return false; 

            meleeTimer = meleeCooldown; 

            if (nextMeleeFirst)
                myAnimator.SetTrigger("Melee1");
            else
                myAnimator.SetTrigger("Melee2");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.meleeClip);

        nextMeleeFirst = !nextMeleeFirst; 


            hits = Physics2D.CircleCastAll(meleeSpawnPoint.position, meleeRange, Vector2.right, 0f, enemyLayer);
            for (int i = 0; i < hits.Length; i++)
            {
                IDamagable damagable = hits[i].collider.gameObject.GetComponent<IDamagable>();
                Debug.Log("Hit: " + hits[i].collider.gameObject.name);
                if (damagable != null)
                {
                    Debug.Log("Damaging: " + hits[i].collider.gameObject.name);
                    damagable.TakeDamage(meleeDamage);
                }
            }
            return true; 
      }

    void UpdateAnimation()
    {
        if (myAnimator == null)
            return;

        bool isFalling =
            myRigidbody.linearVelocity.y < -0.1f;

        bool isRising =
            myRigidbody.linearVelocity.y > 0.1f;

        myAnimator.SetBool("isDashing", isDashing);

        if (isDashing)
        {
            myAnimator.SetBool("isJumping", false);
            return;
        }

        bool shouldJump =
            !isGrounded &&
            (isRising || isFalling);

        myAnimator.SetBool("isJumping", shouldJump);

        bool isRunning =
            isGrounded &&
            Mathf.Abs(moveInput.x) > 0.01f;

        myAnimator.SetBool("isRunning", isRunning);
    }

    bool IsKnockedBack()
    {
        return knockBack != null && knockBack.IsKnockedBack;
    }

   
    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        myAnimator.SetBool("isDashing", true);
        myAnimator.SetBool("isJumping", false);
        myAnimator.SetBool("isRunning", false);

        float originalGravity = myRigidbody.gravityScale;
        myRigidbody.gravityScale = 0f; 
        myRigidbody.linearVelocity = new Vector2(Mathf.Sign(myTransform.localScale.x) * dashSpeed, 0f); 
        yield return new WaitForSeconds(dashDuration);
        myRigidbody.gravityScale = originalGravity; 
        isDashing = false;

        myAnimator.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashCooldown); 
        canDash = true;

    }


    void OnDrawGizmos()
    {
        if (meleeSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeSpawnPoint.position, meleeRange);
        }
    }
}
