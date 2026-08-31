using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ArrowProjectille : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float lifetime = 5f;
    [SerializeField] float damage = 10f;

    private Rigidbody2D Arrowrb;
    private Vector2 Arrowdir;

    private void Awake()
    {
        Arrowrb = GetComponent<Rigidbody2D>();
        Arrowrb.gravityScale = 0f; // Disable gravity for the arrow
    }

    public void Init(Vector2 dir)
    {
        Arrowdir= dir.normalized;
        Vector2 scale = transform.localScale;
        scale.x = Mathf.Sign(dir.x); // Flip the arrow based on direction
        transform.localScale = scale;

        Arrowrb.linearVelocity = Arrowdir * speed;

        Destroy(gameObject, lifetime); // Destroy the arrow after its lifetime
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();
        Debug.Log("Arrow hit: " + collision.gameObject.name);
        if (damagable != null)
        {
            Debug.Log("Arrow damaging: " + collision.gameObject.name);
            damagable.TakeDamage(damage);
            Destroy(gameObject); // Destroy the arrow after hitting an enemy
        }
    }
}
