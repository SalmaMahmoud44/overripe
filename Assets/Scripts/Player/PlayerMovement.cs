using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;  

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float jumpSpeed = 15f;
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
        Run();
        Flip();
    }
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    void OnJump(InputValue value)
    {
        if(value.isPressed && myCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidbody.linearVelocity += new Vector2(0f,jumpSpeed);
        }
    }
    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * moveSpeed, myRigidbody.linearVelocity.y);
        myRigidbody.linearVelocity = playerVelocity;
    }

    void Flip()
    {
        bool isMoving = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon;
        if (isMoving) 
          myTransform.localScale = new Vector2(Mathf.Sign(myRigidbody.linearVelocity.x), 1f);
    }
}
