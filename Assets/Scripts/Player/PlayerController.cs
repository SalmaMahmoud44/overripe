using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float jumpSpeed = 15f;
    [SerializeField] float footstepInterval = 0.5f; // Interval between footstep sounds

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
            shootTimer -= Time.deltaTime; // Decrease the shoot timer
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
            moveInput = Vector2.zero; // Ignore input if controls are locked
            return;
        }
        if (moveInput.x != 0f)
        {
            KeyCode keyPressed = moveInput.x > 0f ? KeyCode.D : KeyCode.A;
            OnPlayerMoved?.Invoke(keyPressed); // Invoke the event when the player moves
        }     
    }
    void OnJump(InputValue value)
    {
        if (isDashing)
            return; // Don't allow jumping while dashing

        if (controlsLocked)
        {
            return;
        }

        if (value.isPressed && myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidbody.linearVelocity += new Vector2(0f,jumpSpeed);

            myAnimator.SetBool("isJumping", true);

            playerAudio.PlayJump(); // Play jump sound effect

            OnPlayerJumped?.Invoke(); // Invoke the event when the player jumps
        }
    }
    void OnDash(InputValue value)
    {
        if (!canDash)
            return; // Don't allow dashing if on cooldown

        if (controlsLocked)
        {
            return;
        }


        if (value.isPressed && !isDashing)
        {
            StartCoroutine(Dash());
            OnPlayerDashed?.Invoke(); // Invoke the event when the player dashes
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
            return; // Only perform melee attack on button press

        if (controlsLocked)
        {
            return;
        }

        if (MeleeAttack())
        {
            OnPlayerMelee?.Invoke(); // Invoke the event when the player performs a melee attack

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
            return; // Don't allow normal movement while dashing

        Vector2 playerVelocity = new Vector2(moveInput.x * moveSpeed, myRigidbody.linearVelocity.y);
        myRigidbody.linearVelocity = playerVelocity;

        bool isMoving = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", isMoving);

        if(isMoving && myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                playerAudio.PlayFootstep(); // Play footstep sound effect
                footstepTimer = 0f; // Reset the footstep timer
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
            return; // Don't allow shooting if on cooldown

        shootTimer = arrowCooldown; // Reset the shoot timer

        //Vector2 shootDirection = new Vector2(Mathf.Sign(myTransform.localScale.x), 0f); // Shoot in the direction the player is facing

        worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); // Get the mouse position in world coordinates
        mousePos = new Vector2(worldPos.x, worldPos.y); // Get the mouse position in 2D space
        Vector2 shootDirection = (mousePos - (Vector2)arrowSpawnPoint.position).normalized; // Shoot towards the mouse position
        ArrowProjectille arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        arrow.Init(shootDirection);
    }   

      bool MeleeAttack()
    {
        if (meleeTimer > 0f)
            return false; // Don't allow melee attack if on cooldown

        meleeTimer = meleeCooldown; // Reset the melee timer

        if (nextMeleeFirst)
            myAnimator.SetTrigger("Melee1");
        else
            myAnimator.SetTrigger("Melee2");

        playerAudio.PlayMelee(); // Play melee sound effect

        nextMeleeFirst = !nextMeleeFirst; // Toggle the melee attack sequence


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
        return true; // Return true to indicate that the melee attack was performed
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
        myRigidbody.gravityScale = 0f; // Disable gravity during dash
        myRigidbody.linearVelocity = new Vector2(Mathf.Sign(myTransform.localScale.x) * dashSpeed, 0f); // Set dash velocity
        yield return new WaitForSeconds(dashDuration); // Wait for the duration of the dash
        myRigidbody.gravityScale = originalGravity; // Restore original gravity
        isDashing = false;

        myAnimator.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashCooldown); // Wait for the cooldown period
        canDash = true;

    }
}
