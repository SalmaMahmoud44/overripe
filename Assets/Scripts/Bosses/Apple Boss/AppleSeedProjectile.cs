using UnityEngine;

public class AppleSeedProjectile : MonoBehaviour
{
    [Header("Seed Movement")]
    [SerializeField] float speed = 10f;
    [SerializeField] float lifeTime = 5f;

    [Header("Damage")]
    [SerializeField] int damage = 8;


    [Header("Knockback")]
    [SerializeField] float knockbackForce = 2f;
    [SerializeField] float knockbackUpwardForce = 1f;

    Rigidbody2D rb;
    Vector2 direction;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }
    private void Start()
    {
   
        Destroy(gameObject ,lifeTime);
    }

    public void Initialize(Vector2 target)
    {
        direction = (target - (Vector2)transform.position).normalized;

        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x)* Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f,0f,angle);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player"))
            return;

        IDamagable damagable = collision.GetComponent<IDamagable>();

        if (damagable != null)
        {
            damagable.TakeDamage(damage);
        }

        KnockBack knockBack = collision.GetComponentInParent<KnockBack>();

        if (knockBack != null && knockBack.CanReceiveKnockback)
        {
            Vector2 knockbackDirection = direction;

            knockBack.ApplyKnockback(direction, knockbackForce, knockbackUpwardForce);

        }

        Destroy(gameObject);
    }

}
