using UnityEngine;
using UnityEngine.UIElements;


public class TripleBFollowPlayer : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform followPoint;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float catchUpSpeed = 10f;

    [SerializeField] float stopDistance = 0.5f;
    [SerializeField] float catchUpDistance = 3f;

    [SerializeField] PlayerController playerController;

    [Header("Smooth Settings")]
    [SerializeField] float smoothTime = 0.2f;

    [Header("Offset Settings")]
    [SerializeField] float verticalMultiplier = 0.5f;

    Rigidbody2D rb;
    Vector2 smoothVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
       
    }
 

    private void FixedUpdate()
    {
        
        FollowPlayer();
    }

    private void Update()
    {
        Flip();
    }
    void FollowPlayer()
    {
        if (followPoint == null)
        {
            Debug.LogWarning("Follow point is not assigned.");
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 offset = (Vector2)followPoint.position - rb.position;
        float distance = offset.magnitude;

        if (distance <= stopDistance)
        {
            rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, Vector2.zero, ref smoothVelocity, smoothTime);
            return;
        }

        offset.y *= verticalMultiplier;

        Vector2 direction = offset.normalized;

        float currentSpeed = (distance >= catchUpDistance) ? catchUpSpeed : moveSpeed;
        Vector2 targetVelocity = direction * currentSpeed;

        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref smoothVelocity, smoothTime);
    }

    void Flip()
    {
        if (playerController == null)
            playerController = GameObject.Find("Player").GetComponent<PlayerController>();


        if (playerController.isFacingRight && transform.localScale.x < 0f)
        {
            transform.localScale = new Vector2(1f, 1f);
        }
        else if (!playerController.isFacingRight && transform.localScale.x > 0f)
        {
            transform.localScale = new Vector2(-1f, 1f);
        }


    }
}
