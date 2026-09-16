using UnityEngine;

public class Saw : MonoBehaviour
{
    [SerializeField] private float damage = 3f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();

        if (damagable != null)
        {
            damagable.TakeDamage(damage);
        }
    }
}
