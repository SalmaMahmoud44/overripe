using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;
    private Rigidbody2D rb;
    private Transform currentPoint;
    public float speed;

    [Header("Damage Settings")]
    [SerializeField] float damageAmount = 5f;
    [SerializeField] float damageCooldown = 1f;
    float damageTimer = 0f;

    [Header("Knockback")]
    [SerializeField] float knockbackForce = 5f;
    [SerializeField] float knockbackUpwardForce = 1.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPoint = pointB.transform;
    }

    void Update()
    {
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;

        Vector2 point = currentPoint.position - transform.position;

        if (currentPoint == pointB.transform)
        {
            rb.linearVelocity = new Vector2(speed, 0);
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed, 0);
        }

        if (Mathf.Abs(transform.position.x - pointB.transform.position.x) < 0.5f && currentPoint == pointB.transform)
        {
            Flip();
            currentPoint = pointA.transform;
        }
        if (Mathf.Abs(transform.position.x - pointA.transform.position.x) < 0.5f && currentPoint == pointA.transform)
        {
            Flip();
            currentPoint = pointB.transform;
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
                    Vector2 direction =(Vector2)collision.transform.position - (Vector2)transform.position;

                    direction = new Vector2(Mathf.Sign(direction.x),0f );

                    knockBack.ApplyKnockback(direction,knockbackForce,knockbackUpwardForce);
                }

                damageTimer = damageCooldown;
            }
        }
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, 0.5f);
        Gizmos.DrawWireSphere(pointB.transform.position, 0.5f);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
    }
}
