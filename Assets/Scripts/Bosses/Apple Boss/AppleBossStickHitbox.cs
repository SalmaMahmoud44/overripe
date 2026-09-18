using UnityEngine;

public class AppleBossStickHitbox : MonoBehaviour
{
    [Header("Hit Damage")]
    [SerializeField] float damage = 10f;

    [Header("Knockback")]
    [SerializeField] float knockbackForce = 16f;
    [SerializeField] float knockbackUpwardForce = 6f;

    bool active ;
    bool playerHit;

    public void Activate()
    {
        active = true;
        playerHit = false;

        Debug.Log(
             $"ACTIVATE -> {gameObject.name} | ID: {GetInstanceID()} | Active: {active}"
         );
    }
    public void Deactivate()
    {
        active = false;
        playerHit = false;

        Debug.Log(
            $"DEACTIVATE -> {gameObject.name} | ID: {GetInstanceID()} | Active: {active}"
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(
            $"TRIGGER -> {gameObject.name} | ID: {GetInstanceID()} | " +
            $"Collision: {collision.name} | Active: {active}"
        );
        if (!active) return;

        if(playerHit) return;

        if(!collision.CompareTag("Player"))
            return;

        Debug.Log("STAFF HIT PLAYER!");
        IDamagable damagable = collision.GetComponent<IDamagable>();

        if(damagable != null)
        {
            damagable.TakeDamage(damage);
        }

        KnockBack knockBack = collision.GetComponent<KnockBack>();

        if(knockBack != null && knockBack.CanReceiveKnockback)
        {
            Vector2 direction = (collision.transform.position - transform.position).normalized;

            knockBack.ApplyKnockback(direction,knockbackForce,knockbackUpwardForce);
        }

        playerHit = true;
    }

  
}
