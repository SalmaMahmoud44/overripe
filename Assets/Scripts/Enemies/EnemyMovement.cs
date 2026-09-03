using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;
    private Rigidbody2D rb;
    private Transform currentPoint;
    public float speed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPoint = pointB.transform;
    }

    void Update()
    {
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
            Debug.Log("Reached B, switching to A");
            Flip();
            currentPoint = pointA.transform;
        }
        if (Mathf.Abs(transform.position.x - pointA.transform.position.x) < 0.5f && currentPoint == pointA.transform)
        {
            Debug.Log("Reached A, switching to B");
            Flip();
            currentPoint = pointB.transform;
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