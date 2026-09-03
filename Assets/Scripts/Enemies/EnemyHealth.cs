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
    [SerializeField] float deathDelay = 0.5f;

    Animator anim;
    bool isDead = false;

    public event Action OnEnemyDied;

    void Start()
    {
        currentHealth = maxHealth;
        rotTimer = GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();
        anim = GetComponent<Animator>();
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
            anim.SetTrigger("Hit");
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");

        rotTimer.AddTime(timeToAdd);
        OnEnemyDied?.Invoke();

        GetComponent<EnemyMovement>().enabled = false;

        Destroy(gameObject, deathDelay);
    }
}