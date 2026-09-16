using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DustPuff : MonoBehaviour
{
    [SerializeField] float speed = 3f;
    [SerializeField] Collider2D roomBounds;

    Rigidbody2D rb;
    float minX;
    float maxX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Init(Vector2 direction, Collider2D bounds)
    {
        rb.linearVelocity = direction.normalized * speed;

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

        Debug.Log("Player hit by dust puff - fog effect placeholder");

        Destroy(gameObject);
    }
}