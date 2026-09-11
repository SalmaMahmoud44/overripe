using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamagable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Time Settings")]
    [SerializeField] private float timeToAdd = 2f;
    [SerializeField] RotTimer rotTimer;

    [Header("Death Settings")]
    [SerializeField] float deathDelay = 2f;
    [SerializeField] float flyingDeathGravityScale = 3f;

   
    Animator anim;
    bool isDead = false;

    Collider2D col;
    Rigidbody2D rb;
    SoliderHitFeedback hitFeedback;

    public event Action OnEnemyDied;

    void Start()
    {
        currentHealth = maxHealth;
        rotTimer = GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        hitFeedback = GetComponent<SoliderHitFeedback>();
    }

    public new void TakeDamage(float damage)
    {
        if (isDead)
            return;

        Debug.Log("Enemy took damage: " + damage);
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if(anim != null)
              anim.SetTrigger("Hit");
            if (hitFeedback != null)
                hitFeedback.PlayHitFeedback();
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");

        rotTimer.AddTime(timeToAdd);
        OnEnemyDied?.Invoke();

        EnemyMovement movement = GetComponent<EnemyMovement>();
        if (movement != null)
            movement.enabled = false;

        FlyEnemy flyEnemy = GetComponent<FlyEnemy>();
        if (flyEnemy != null)
        {
            flyEnemy.OnDeath();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = flyingDeathGravityScale;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            col.isTrigger = true;
            rb.bodyType = RigidbodyType2D.Static;
        }

        Destroy(gameObject, deathDelay);
    }
}