using System;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamagable
{
    [Header("Health")]
    [SerializeField] float maxHealth = 100f;
    

    [Header("Vulnerable")]
    [SerializeField] float vulnerableDamageMultiplier = 1.5f;
    [SerializeField] bool damageOnlyWhenVulnerable = false;
    

    float currentHealth;
    bool isVulnerable = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsVulnerable => isVulnerable;

    public event Action<float> OnDamaged;
    public event Action OnHit;
    public event Action OnDied;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if(IsDead) return;

        if (amount <= 0f) return;

        if (damageOnlyWhenVulnerable && !isVulnerable) 
            return;

        float finalDamage = isVulnerable ? amount * vulnerableDamageMultiplier : amount;

        currentHealth -= finalDamage;
        
        currentHealth = Mathf.Max(currentHealth, 0f);
        Debug.Log("Have been damaged");
        OnHit?.Invoke();

        OnDamaged?.Invoke(GetHealthPercent());

        if (IsDead)
            OnDied?.Invoke();
    }

    public void SetVulnerable(bool value)
    {
        if (IsDead) return;

        isVulnerable = value;
    }

    public float GetHealthPercent()
    {
        if(maxHealth <= 0f) return 0f;
        return currentHealth / maxHealth;
    } 

}
