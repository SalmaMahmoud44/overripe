using System.Collections;
using UnityEngine;

public class FlyEnemy : MonoBehaviour,ILaserStunnable
{
    public float speed;
    public float lineOfSite;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 5f;
    [SerializeField] float damageCooldown = 1f;
    [SerializeField] float knockbackDuration = 0.12f;

    float damageTimer = 0f;

    [Header("Knockback")]
    [SerializeField] float knockbackForce = 5f;
    [SerializeField] float knockbackUpwardForce = 2f;

    private Transform player;
    private Animator animator;

    private bool isDead = false;
    private bool isAttacking = false;
    private bool isLaserStunned = false;
    private float attackTimer = 0f;

    private bool isKnockedBack = false;
    private Coroutine knockbackRoutine;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();

    }

    void Update()
    {
        if (isDead) return;
;
        if (isLaserStunned)
            return;
        
        if (isKnockedBack)
            return;

        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;

        float distanceFromPlayer = Vector2.Distance(player.position, transform.position);

        if (distanceFromPlayer < attackRange)
        {
            if (!isAttacking)
            {
                isAttacking = true;
                attackTimer = 0f;
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                animator.SetTrigger("Attack");
                attackTimer = attackCooldown;
            }
        }
        else
        {
            if (isAttacking)
            {
                isAttacking = false;
                animator.SetTrigger("StopAttack");
            }

            if (distanceFromPlayer < lineOfSite)
            {
                transform.position = Vector2.MoveTowards(this.transform.position, player.position, speed * Time.deltaTime);
                Flip();
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && damageTimer <= 0f)
        {
            IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(damageAmount);
                KnockBack knockBack = collision.gameObject.GetComponent<KnockBack>();

                if (knockBack != null)
                {
                    Vector2 direction =(Vector2)collision.transform.position -(Vector2)transform.position;

                    direction = new Vector2(Mathf.Sign(direction.x),0f);

                    knockBack.ApplyKnockback(direction,knockbackForce,knockbackUpwardForce);
                }

                damageTimer = damageCooldown;
            }
        }
    }

    public void ApplyHitPush(Vector2 direction)
    {
        if(isDead) return;
        if(knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(HitPushRoutine(direction));

    }

    IEnumerator HitPushRoutine(Vector2 direction)
    {
        isKnockedBack = true;

        direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.left;

        float timer =0f;

        while (timer < knockbackDuration)
        {
            Vector2 movement = direction * knockbackForce;

            transform.position += (Vector3)(movement * Time.deltaTime);

            timer += Time.deltaTime;

            yield return null;
        }
        isKnockedBack = false;
        knockbackRoutine = null;
    }
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (player.position.x > transform.position.x ? 1f : -1f);
        transform.localScale = scale;
    }

    public void OnDeath()
    {
        isDead = true;
        enabled = false; 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lineOfSite);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void StartLaserStun()
    {
        if (isDead)
            return;

        isLaserStunned = true;

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }

        isKnockedBack = false;
        isAttacking = false;

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("StopAttack");
        }
    }

    public void EndLaserStun()
    {
        if (!isLaserStunned)
            return;

        isLaserStunned = false;

        isAttacking = false;
    }
}