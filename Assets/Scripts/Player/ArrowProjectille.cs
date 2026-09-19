using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ArrowProjectille : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float lifetime = 5f;
    [SerializeField] float damage = 10f;

    [SerializeField] private SpriteRenderer arrowSprite;
    [SerializeField] GameObject hitEffectPrefab;

    private Rigidbody2D arrowrb;
    private Vector2 arrowDir;



    private void Awake()
    {
        arrowrb = GetComponent<Rigidbody2D>();
        arrowrb.gravityScale = 0f;

        if(arrowSprite == null )
          arrowSprite = GetComponentInChildren<SpriteRenderer>();
    }

    public void Init(Vector2 dir)
    {
        arrowDir = dir.normalized;

        float angle = Mathf.Atan2(arrowDir.y, arrowDir.x) * Mathf.Rad2Deg;

        if (arrowDir.x >= 0)
        {
    
            arrowSprite.flipY = false;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 50f);
        }
        else
        {
            arrowSprite.flipY = true;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + 50f);
        }

        arrowrb.linearVelocity = arrowDir * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Arrow hit: " + collision.gameObject.name);


        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy"))
            return;

        IDamagable damagable = collision.GetComponentInParent<IDamagable>();

        if (damagable != null)
        {
            Debug.Log("Arrow damaging: " + collision.gameObject.name);

            damagable.TakeDamage(damage);

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, collision.transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
