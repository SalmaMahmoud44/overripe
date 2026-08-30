using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;  

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float jumpSpeed = 15f;

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed = 20f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float dashCooldown = 1f;

    [Header("Shoot Settings")]
    [SerializeField] ArrowProjectille arrowPrefab;
    [SerializeField] Transform arrowSpawnPoint;
    [SerializeField] float arrowCooldown = 0.5f;


    float shootTimer = 0f;  
    bool isDashing = false;
    bool canDash = true;
    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Transform myTransform;
    CapsuleCollider2D myCollider;
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myTransform = GetComponent<Transform>();
        myCollider = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        if (shootTimer > 0f)
            shootTimer -= Time.deltaTime; // Decrease the shoot timer


        Run();
        Flip();
    }
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    void OnJump(InputValue value)
    {
        if (isDashing)
            return; // Don't allow jumping while dashing

        if (value.isPressed && myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidbody.linearVelocity += new Vector2(0f,jumpSpeed);
        }
    }
    void OnDash(InputValue value)
    {
        if (!canDash)
            return; // Don't allow dashing if on cooldown

        if (value.isPressed && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    void OnShoot(InputValue value)
    {
        

        if (value.isPressed)
        {
            ShootArrow();
        }
    }
    void OnMelee(InputValue value)
    {
        if (value.isPressed)
        {
            // Implement melee attack logic here
            Debug.Log("Melee Attack!");
        }
    }
    void Run()
    {
        if(isDashing) 
            return; // Don't allow normal movement while dashing
  
        Vector2 playerVelocity = new Vector2(moveInput.x * moveSpeed, myRigidbody.linearVelocity.y);
        myRigidbody.linearVelocity = playerVelocity;
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

        Vector2 shootDirection = new Vector2(Mathf.Sign(myTransform.localScale.x), 0f); // Shoot in the direction the player is facing
        ArrowProjectille arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        arrow.Init(shootDirection);
    }   
    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        float originalGravity = myRigidbody.gravityScale;
        myRigidbody.gravityScale = 0f; // Disable gravity during dash
        bool isMoving = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        myRigidbody.linearVelocity = new Vector2(Mathf.Sign(myTransform.localScale.x) * dashSpeed, 0f); // Set dash velocity
        yield return new WaitForSeconds(dashDuration); // Wait for the duration of the dash
        myRigidbody.gravityScale = originalGravity; // Restore original gravity
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown); // Wait for the cooldown period
        canDash = true;

    }
}
