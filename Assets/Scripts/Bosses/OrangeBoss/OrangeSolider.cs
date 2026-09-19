using System;
using UnityEngine;

public class OrangeSolider : MonoBehaviour,ILaserStunnable
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
    KnockBack knockBack;

    float attackTimer;
    float walkSoundTimer;

    bool isAttacking;
    bool isDead;
    bool isLaserStunned;

    public event Action OnDied;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (hitFeedback == null)
            hitFeedback = GetComponent<SoliderHitFeedback>();

        animator = GetComponentInChildren<Animator>();

        knockBack = GetComponent<KnockBack>();
    }

    public void SetTarget(Transform playerTransform)
    {
        target = playerTransform;

        if (target != null)
           targetDamagable = target.GetComponent<IDamagable>();
    }

    private void Update()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (isLaserStunned)
        {
            StopMoving();
            isAttacking = false;
            return;
        }

        if (target == null || isDead)
            return;

        if (knockBack != null && knockBack.IsHitPushed)
        {
            if (animator != null)
                animator.SetBool("isWalking", false);

            return;
        }

        if (hitFeedback != null && !hitFeedback.CanAct)
        {
            StopMoving();
            return;
        }

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > attackRange)
        {
            WalkTowardsPlayer();
        }
        else
        {

            StopMoving();
            TryAttack();      
        }
    }
    void WalkTowardsPlayer()
    {
        if (animator != null)
            animator.SetBool("isWalking", true);

        walkSoundTimer -= Time.deltaTime;

        if (walkSoundTimer <= 0f && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.soldierWalkClip);
            walkSoundTimer = 0.4f;   
        }

        float directionX = Mathf.Sign(target.position.x - transform.position.x);

        rb.linearVelocity = new Vector2(directionX * moveSpeed, rb.linearVelocity.y);

        Flip(-directionX);
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0f,rb.linearVelocity.y);

        if(animator != null)
            animator.SetBool("isWalking",false);
    }
  
    void TryAttack()
    {
        if (attackTimer > 0f || isAttacking)
            return;

        attackTimer = attackCooldown;
        isAttacking = true;

        if (animator != null)
            animator.SetTrigger("Attack");
    }
    public void DealAttackDamage()
    {
       if(target == null || isDead) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.soldierAttackClip);

        float distance = Vector2.Distance(transform.position, target.position);

        if(distance > attackRange+0.25f)
            return;

        Debug.Log("Orange Soldier ATTACK HIT!");

        if (targetDamagable != null)
            targetDamagable.TakeDamage(attackDamage);

        KnockBack knockBack = target.GetComponent<KnockBack>();

        if (knockBack != null)
        {
            Vector2 direction =target.position -transform.position;

            direction = new Vector2(Mathf.Sign(direction.x),0f );

            knockBack.ApplyKnockback(direction,knockbackForce,knockbackUpwardForce);
        }
    }
    public void FinishAttack()
    {
        isAttacking = false;
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
        isDead = true;
        OnDied?.Invoke();
    }

    public void StartLaserStun()
    {
        if(isDead) 
            return;

        isLaserStunned = true;

        isAttacking = false ;

        if(rb != null)
            rb.linearVelocity = Vector3.zero;

        if(animator != null)
        {
            animator.SetBool("isWalking",false);
            animator.ResetTrigger("Attack");
        }
    }

    public void EndLaserStun()
    {
        if(!isLaserStunned)
            return;

        isLaserStunned = false; 

        if(rb != null)
            rb.linearVelocity =Vector2.zero;

        if (animator != null)
            animator.SetBool("isWalking", false);
    }
}