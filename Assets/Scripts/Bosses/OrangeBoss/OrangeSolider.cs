using System;
using UnityEngine;

public class OrangeSolider : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 3f;

    [Header("Attack Settings")]
    [SerializeField] float attackRange = 0.8f;
    [SerializeField] float attackDamage = 5f;
    [SerializeField] float attackCooldown = 1.5f;

    [Header("Hit Feedback")]
    [SerializeField] SoliderHitFeedback hitFeedback;

    [Header("Knockback")]
    [SerializeField] float knockbackForce = 6f;
    [SerializeField] float knockbackUpwardForce = 2f;

    Transform target;
    IDamagable targetDamagable;

    Rigidbody2D rb;
    Animator animator;

    float attackTimer;

    public event Action OnDied;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (hitFeedback == null)
            hitFeedback = GetComponent<SoliderHitFeedback>();

        animator = GetComponentInChildren<Animator>();
    }

    public void SetTarget(Transform playerTransform)
    {
        target = playerTransform;
        targetDamagable = playerTransform.GetComponent<IDamagable>();
    }

    private void Update()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (target == null)
            return;

        if (hitFeedback != null && !hitFeedback.CanAct)
        {
            rb.linearVelocity = Vector2.zero;

            if (animator != null)
                animator.SetBool("isWalking", false);

            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > attackRange)
        {
      
            if (animator != null)
                animator.SetBool("isWalking", true);

            float directionX = Mathf.Sign(target.position.x - transform.position.x);

            rb.linearVelocity = new Vector2(directionX * moveSpeed,rb.linearVelocity.y);

            Flip(-directionX);
        }
        else
        {
            rb.linearVelocity = new Vector2(  0f, rb.linearVelocity.y );

            if (animator != null)
                animator.SetBool("isWalking", false);

            Attack();
        }
    }

    void Attack()
    {
        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;

        if (animator != null)
            animator.SetTrigger("Attack");

        if (targetDamagable != null)
            targetDamagable.TakeDamage(attackDamage);

        KnockBack knockBack = target.GetComponent<KnockBack>();

        if (knockBack != null)
        {
            Vector2 direction =(Vector2)target.position -(Vector2)transform.position;

            direction = new Vector2(Mathf.Sign(direction.x),0f );

            knockBack.ApplyKnockback(direction,knockbackForce,knockbackUpwardForce);
        }
    }

    void Flip(float directionX)
    {
        if (Mathf.Abs(directionX) < 0.01f)
            return;

        Vector3 scale = transform.localScale;

        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(directionX);

        transform.localScale = scale;
    }

    public void NotifyDeath()
    {
        OnDied?.Invoke();
    }
}