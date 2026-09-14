using System;
using UnityEngine;

public class OrangeSoliderHealth : MonoBehaviour, IDamagable
{
    [Header("Health")]
    [SerializeField] float maxHealth = 30f;
    [SerializeField] float currentHealth;

    [Header("Death")]
    [SerializeField] float deathDelay = 1.0f;


    [SerializeField] RotTimer rotTimer;
    [SerializeField] float timeToAdd = 5f;

    OrangeSolider solider;
    SoliderHitFeedback hitFeedback;

    Animator animator;
    Collider2D soliderCollider;
    Rigidbody2D soliderRigidbody;

    bool isDead = false;

    public event Action OnDied;

    private void Awake()
    {
        currentHealth = maxHealth;

        solider = GetComponent<OrangeSolider>();
        hitFeedback = GetComponent<SoliderHitFeedback>();

        animator = GetComponentInChildren<Animator>();
        soliderCollider = GetComponent<Collider2D>();
        soliderRigidbody = GetComponent<Rigidbody2D>();

        if(rotTimer == null)
            rotTimer = GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();

    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log($"Orange Soldier took {damage} damage. HP = {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            if (hitFeedback != null)
                hitFeedback.PlayHitFeedback();
        }
    }

    void Die()
    {
        if(isDead) return;

        isDead = true;

        if (solider != null)
        {
            solider.NotifyDeath();
            solider.enabled = false;
        }
            

        if (hitFeedback != null)
            hitFeedback.StopFeedback();

        if (soliderCollider != null)
            soliderCollider.isTrigger = true;

        if(soliderRigidbody != null)
            soliderRigidbody.bodyType = RigidbodyType2D.Static;

        if (animator != null)
            animator.SetTrigger("Die");

        OnDied?.Invoke();

        rotTimer.AddTime(timeToAdd);

        Destroy(gameObject,deathDelay);
    }

    public bool IsDead()
    {
        return isDead;
    }
}
