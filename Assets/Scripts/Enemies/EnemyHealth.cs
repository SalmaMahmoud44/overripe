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

    public event Action OnEnemyDied;
    void Start()
    {
        currentHealth = maxHealth;
        rotTimer = GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();
    }

    public new void TakeDamage(float damage)
    {
        Debug.Log("Enemy took damage: " + damage);
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        rotTimer.AddTime(timeToAdd);
        Destroy(gameObject);
        OnEnemyDied?.Invoke();
    }
}
