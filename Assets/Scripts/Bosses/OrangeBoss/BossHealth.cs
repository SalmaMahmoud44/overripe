using System;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamagable
{
    [SerializeField] float maxHealth = 500f;
    [SerializeField] float vulnerableDamageMultiplier = 1.5f; 

    float currentHealth;
    bool isVulnerable = false;

    public event Action<float> OnDamaged;
    public event Action OnDied;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        float finalDamage = isVulnerable ? amount * vulnerableDamageMultiplier : amount;
        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        OnDamaged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0f)
            OnDied?.Invoke();
    }

    public void SetVulnerable(bool value)
    {
        isVulnerable = value;
    }

    public float GetHealthPercent() => currentHealth / maxHealth;

}
