using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float jumpSpeed = 15f;
    [SerializeField] float footstepInterval = 0.5f; 

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




    // Events for player actions
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
    Rigidbody2D myRigidbody;
    Transform myTransform;
    CapsuleCollider2D myCollider;
    Vector2 worldPos;
    Vector2 mousePos;
    RaycastHit2D[] hits;
    Animator myAnimator;
    PlayerAudio playerAudio;



    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myTransform = GetComponent<Transform>();
        myCollider = GetComponent<CapsuleCollider2D>();
        arrowSpawnPoint = transform.Find("ArrowSpawnPoint");
        meleeSpawnPoint = transform.Find("MeleeSpawnPoint");
        enemyLayer = LayerMask.GetMask("Enemy");
        myAnimator = GetComponentInChildren<Animator>();
        playerAudio = GetComponent<PlayerAudio>();

        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();
    }

    void Update()
    {
        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime; 
        if(meleeTimer > 0f)
            meleeTimer -= Time.deltaTime;


        Run();
        Flip();
        CheckJumpAnimation();
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
        if (isDashing)
            return; 

        if (controlsLocked)
        {
            return;
        }

        if (value.isPressed && myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidbody.linearVelocity += new Vector2(0f,jumpSpeed);

            myAnimator.SetBool("isJumping", true);

            playerAudio.PlayJump(); 

            OnPlayerJumped?.Invoke(); 
        }
    }
    void OnDash(InputValue value)
    {
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
        if(!value.isPressed)
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

    public void SetControlsLocked(bool locked)
    {
        controlsLocked = locked;

        if (locked)
        {
            moveInput = Vector2.zero;

            if (myRigidbody != null)
            {
                myRigidbody.linearVelocity = new Vector2(
                    0f,
                    myRigidbody.linearVelocity.y
                );
            }
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

        if(isMoving && myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                playerAudio.PlayFootstep(); 
                footstepTimer = 0f; 
            }
        }
    }

    void Flip()
    {
        bool isMoving = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        if (isMoving) 
          myTransform.localScale = new Vector2(Mathf.Sign(myRigidbody.linearVelocity.x), 1f);
    }

    void ShootArrow()
    {
        if(arrowPrefab == null || arrowSpawnPoint == null)
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

        playerAudio.PlayMelee(); 

        nextMeleeFirst = !nextMeleeFirst; 


        hits = Physics2D.CircleCastAll(meleeSpawnPoint.position, meleeRange, Vector2.right, 0f, enemyLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            IDamagable damagable = hits[i].collider.gameObject.GetComponent<EnemyHealth>();
            Debug.Log("Hit: " + hits[i].collider.gameObject.name);
            if (damagable != null)
            {
                Debug.Log("Damaging: " + hits[i].collider.gameObject.name);
                damagable.TakeDamage(meleeDamage);
            }
        }
        return true; 
    }
    void CheckJumpAnimation()
    {
        if (myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) && myRigidbody.linearVelocity.y <= 0f)
        {
            myAnimator.SetBool("isJumping", false);
        }
    }


    void OnDrawGizmos()
    {
        if (meleeSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeSpawnPoint.position, meleeRange);
        }
    }
    System.Collections.IEnumerator Dash()
    {
        isDashing = true;

        myAnimator.SetBool("isDashing", true);
        canDash = false;
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
}
