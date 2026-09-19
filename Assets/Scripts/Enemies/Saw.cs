using UnityEngine;

public class Saw : MonoBehaviour
{
    [SerializeField] private float damage = 3f;

    [Header("Knockback")]
    [SerializeField] float knockbackForce = 5f;
    [SerializeField] float knockbackUpwardForce = 1.5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();

        if (damagable != null)
        {
            damagable.TakeDamage(damage);
        }
        KnockBack knockBack = collision.gameObject.GetComponent<KnockBack>();

        if (knockBack != null)
        {
            Vector2 direction = (Vector2)collision.transform.position - (Vector2)transform.position;

            direction = new Vector2(Mathf.Sign(direction.x), 0f);

            knockBack.ApplyKnockback(direction, knockbackForce, knockbackUpwardForce);
        }
    }
}
