using UnityEngine;

public class FlyEnemy : MonoBehaviour
{
    public float speed;
    public float lineOfSite;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 5f;
    [SerializeField] float damageCooldown = 1f;
    float damageTimer = 0f;

    private Transform player;
    private Animator animator;
    private bool isDead = false;
    private bool isAttacking = false;
    private float attackTimer = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead) return;

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
                damageTimer = damageCooldown;
            }
        }
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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lineOfSite);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}