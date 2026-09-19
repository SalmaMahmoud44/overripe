using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WaveAttack : MonoBehaviour
{
    [SerializeField] float speed = 8f;
    [SerializeField] float damage = 10f;

    [Header("Knockback")]
    [SerializeField] float knockbackForce = 5f;
    [SerializeField] float knockbackUpwardForce = 2f;

    [Header("Visuals")]
    [SerializeField] SpriteRenderer spriteRenderer;

    Rigidbody2D rb;
    Collider2D roomBounds;
    float minX;
    float maxX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Init(float directionX, Collider2D bounds)
    {
        rb.linearVelocity = new Vector2(directionX * speed, 0f);

        if (spriteRenderer != null)
            spriteRenderer.flipX = directionX < 0f;

        roomBounds = bounds;
        if (roomBounds != null)
        {
            minX = roomBounds.bounds.min.x;
            maxX = roomBounds.bounds.max.x;
        }
    }

    void Update()
    {
        if (roomBounds == null)
            return;

        if (transform.position.x <= minX || transform.position.x >= maxX)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        PlayerDeath playerDeath = collision.GetComponent<PlayerDeath>();
        if (playerDeath != null)
        {
            playerDeath.TakeDamage(damage);
        }

        KnockBack knockBack = collision.GetComponent<KnockBack>();
        if (knockBack != null)
        {
            Vector2 direction = (Vector2)collision.transform.position - (Vector2)transform.position;
            direction = new Vector2(Mathf.Sign(direction.x), 0f);

            knockBack.ApplyKnockback(direction, knockbackForce, knockbackUpwardForce);
        }

        Destroy(gameObject);

    }
}