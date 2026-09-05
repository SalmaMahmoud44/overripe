using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MangoBoss : MonoBehaviour, IDamagable
{
    public enum BossState { Idle, Attacking, Dead }

    [Header("Health Settings")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth;
    bool isEnraged = false;

    [Header("UI")]
    [SerializeField] Slider healthSlider;

    [Header("Detection Settings")]
    [SerializeField] float detectRange = 6f;
    Transform player;

    [Header("References")]
    [SerializeField] Animator animator;

    [Header("Jump Shadow Settings")]
    [SerializeField] SpriteRenderer jumpShadow;
    [SerializeField] Vector3 shadowMaxScale = new Vector3(1.5f, 1.5f, 1f);
    [SerializeField] Color shadowFadedColor = new Color(0f, 0f, 0f, 0.2f);
    [SerializeField] Color shadowClearColor = new Color(0f, 0f, 0f, 0.8f);
    [SerializeField] float shadowGrowDuration = 1f;

    BossState currentState = BossState.Idle;
    Coroutine attackRoutine;

    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            healthSlider.gameObject.SetActive(false);
        }

        if (jumpShadow != null)
            jumpShadow.gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentState == BossState.Dead)
            return;

        Flip();

        if (currentState != BossState.Idle)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectRange)
        {
            StartAttacking();
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (player.position.x > transform.position.x ? -1f : 1f);
        transform.localScale = scale;
    }

    void StartAttacking()
    {
        currentState = BossState.Attacking;

        if (healthSlider != null)
            healthSlider.gameObject.SetActive(true);

        attackRoutine = StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        while (currentState == BossState.Attacking)
        {
            Debug.Log(isEnraged ? "Attack cycle (enraged)" : "Attack cycle (normal)");
            yield return new WaitForSeconds(2f);
        }
    }

    [ContextMenu("Test Shadow Grow")]
    void TestShadowGrow()
    {
        StartCoroutine(GrowShadow(transform.position, shadowGrowDuration));
    }

    IEnumerator GrowShadow(Vector3 landingPosition, float duration)
    {
        jumpShadow.transform.position = landingPosition;
        jumpShadow.transform.localScale = Vector3.zero;
        jumpShadow.color = shadowFadedColor;
        jumpShadow.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            jumpShadow.transform.localScale = Vector3.Lerp(Vector3.zero, shadowMaxScale, progress);
            jumpShadow.color = Color.Lerp(shadowFadedColor, shadowClearColor, progress);

            yield return null;
        }

        jumpShadow.transform.localScale = shadowMaxScale;
        jumpShadow.color = shadowClearColor;
    }

    public new void TakeDamage(float damage)
    {
        if (currentState == BossState.Dead)
            return;

        currentHealth -= damage;
        Debug.Log("Mango Boss took damage: " + damage + " | Health left: " + currentHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (!isEnraged && currentHealth <= maxHealth * 0.5f)
        {
            isEnraged = true;
            Debug.Log("Mango Boss entered enraged phase");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        currentState = BossState.Dead;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        Debug.Log("Mango Boss died");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}