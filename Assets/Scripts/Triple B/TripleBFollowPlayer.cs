using System;
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

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string floatingAnimationName = "TripleBFloat";

    [SerializeField] bool followEnabled = false;


    Rigidbody2D rb;
    Vector2 smoothVelocity;

    TripleBLaser tripleBLaser;

  
    bool reachedPlayer = false;

    public bool IsFollowing => followEnabled;

    public event Action OnReachedPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        tripleBLaser = GetComponent<TripleBLaser>();


        if (animator == null )
            animator = GetComponentInChildren<Animator>();

        if(!followEnabled )
            rb.bodyType = RigidbodyType2D.Static;
    
    }
 

    private void FixedUpdate()
    {
        if (!followEnabled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (tripleBLaser != null && tripleBLaser.IsAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        FollowPlayer();
    }

    private void Update()
    {
        if (!followEnabled)
            return;

        if (tripleBLaser != null && tripleBLaser.IsAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Flip();
    }

    public void StartFollowing()
    {
        followEnabled = true;
        reachedPlayer = false;

        smoothVelocity = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
            
        if (animator != null)
        {
            animator.Play(floatingAnimationName, 0, 0f);
            animator.speed = 1f;
        }
    }

    public void StopFollowing()
    {
        followEnabled = false;

        smoothVelocity = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
          
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

            if (!reachedPlayer) 
            {
                reachedPlayer = true;
                OnReachedPlayer?.Invoke();
            }

            return;
        }

        offset.y *= verticalMultiplier;

        Vector2 direction = offset.normalized;

        float currentSpeed = (distance >= catchUpDistance) ? catchUpSpeed : moveSpeed;
        Vector2 targetVelocity = direction * currentSpeed;

    

        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity,targetVelocity,ref smoothVelocity,smoothTime );
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
