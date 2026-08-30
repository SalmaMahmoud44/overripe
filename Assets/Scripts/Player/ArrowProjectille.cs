using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ArrowProjectille : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float lifetime = 5f;

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

}
