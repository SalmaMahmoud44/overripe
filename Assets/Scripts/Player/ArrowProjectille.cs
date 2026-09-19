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
        Arrowrb.gravityScale = 0f; 
    }

    public void Init(Vector2 dir)
    {
        Arrowdir = dir.normalized;

        float angle = Mathf.Atan2(Arrowdir.y, Arrowdir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler( 0f, 0f, angle - 50f);

        Arrowrb.linearVelocity = Arrowdir * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Arrow hit: " + collision.gameObject.name);

        if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy"))
            return;

        IDamagable damagable =
            collision.GetComponentInParent<IDamagable>();

        if (damagable != null)
        {
            Debug.Log("Arrow damaging: " + collision.gameObject.name);

            damagable.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
