using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;
using UnityEngine;

public class OrangeBossController : MonoBehaviour
{
    public enum BossPhase { Phase1, Phase2 ,Phase3,Frenzy,Dead};
    public enum BossState { Idle,Telegraph , Summon , Charge, Recovery, AttackWindow,Transitioning,Dead };

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Transform[] soliderSpawnPoints;
    [SerializeField] GameObject soliderPrefab;
    [SerializeField] BossHealth bossHealth;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    [SerializeField] string rollingTrigger = "Rolling";
    [SerializeField] string deathTrigger = "Death";

    [Header("Visual Feedback")]
    [SerializeField] Color summonTelegraphColor = Color.yellow;
    [SerializeField] Color chargeTelegraphColor = Color.red;
    [SerializeField] Color recoveryColor = Color.blue;
    [SerializeField] Color phaseTransitionColor = Color.white;
    [SerializeField] float pulseScaleAmount = 0.08f;
    [SerializeField] float pulseSpeed = 6f;

    [Header("Phase Threshold")]
    [SerializeField] float phase2Threshold = 0.5f;
    [SerializeField] float phase3Threshold = 0.2f;
    [SerializeField] float frenzyThreshold = 0.1f;

    [Header("Charge Settings")]
    [SerializeField] float chargeTelegraphTime = 1f;
    [SerializeField] float chargeSpeed = 14f;
    [SerializeField] float chargeMaxDuration = 1.2f;
    [SerializeField] LayerMask groundLayer;

    [Header("Summon Settings")]
    [SerializeField] int solidersToSummonP1 = 2;
    [SerializeField] int soldiersToSummonP2 = 3;
    [SerializeField] int soldiersToSummonP3 = 3;
    [SerializeField] float summonTelegraphTime = 0.6f;

    [Header("Recovery Settings")]
    [SerializeField] float recoveryTimeP1 = 1.8f;
    [SerializeField] float recoveryTimeP2 = 1.3f;
    [SerializeField] float recoveryTimeP3 = 0.9f;
    [SerializeField] float recoveryTimeFrenzy = 0.6f;

    [SerializeField] float attackWindowTime = 2f;

    [Header("Tranition VFX")]
    [SerializeField] GameObject phaseTransitionVFX;
    [SerializeField] float phaseTransitionDuration = 1.5f;

    public event Action<BossPhase> OnPhaseChanged;
    public event Action OnBossDied;

    public BossPhase currentPhase { get; private set; } = BossPhase.Phase1;
    public BossState currentState { get; private set; } = BossState.Idle;

    Coroutine fightRoutine;
    Coroutine visualFeedbackRoutine;
    Vector3 baseScale;
    Color baseColor;
    bool isDead = false;
    int currentSoldierCount = 0;


    private void Awake()
    {
        if(bossHealth == null) 
            bossHealth = GetComponent<BossHealth>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        baseScale = transform.localScale;
        baseColor = spriteRenderer!=null ? spriteRenderer.color : Color.white;
    }

    private void OnEnable()
    {
        bossHealth.OnDamaged += HandleDamaged;
        bossHealth.OnDied += HandleDied;
    }
    void OnDisable()
    {
        bossHealth.OnDamaged -= HandleDamaged;
        bossHealth.OnDied -= HandleDied;
    }

    void Start()
    {
        fightRoutine = StartCoroutine(FightLoop());
    }

    IEnumerator FightLoop()
    {
        while (!isDead) 
        {
            yield return StartCoroutine(RunPattern(currentPhase));

            yield return StartCoroutine(PlayerAttackWindow());
        }
    }

    IEnumerator RunPattern(BossPhase phase)
    {
        switch (phase) 
        { 
            case BossPhase.Phase1:
                yield return StartCoroutine(DoSummon(solidersToSummonP1));
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(DoCharge(chargeSpeed, chargeTelegraphTime));
                yield return StartCoroutine(DoRecovery(recoveryTimeP1));
                break;
            case BossPhase.Phase2:
                if (UnityEngine.Random.value < 0.5f)
                {
                    yield return StartCoroutine(DoSummon(soldiersToSummonP2));
                    yield return StartCoroutine(DoCharge(chargeSpeed * 1.15f, chargeTelegraphTime * 0.9f));
                }
                else
                {
                    yield return StartCoroutine(DoCharge(chargeSpeed * 1.15f, chargeTelegraphTime * 0.9f));
                    yield return StartCoroutine(DoSummon(soldiersToSummonP2));
                }
                yield return StartCoroutine(DoRecovery(recoveryTimeP2));
                break;
            case BossPhase.Phase3:
                yield return StartCoroutine(DoSummon(soldiersToSummonP3));
                yield return StartCoroutine(DoCharge(chargeSpeed * 1.3f, chargeTelegraphTime * 0.75f));
                yield return StartCoroutine(DoCharge(chargeSpeed * 1.3f, chargeTelegraphTime * 0.75f));
                yield return StartCoroutine(DoRecovery(recoveryTimeP3));
                break;

            case BossPhase.Frenzy:
                yield return StartCoroutine(DoSummon(soldiersToSummonP3));
                yield return StartCoroutine(DoCharge(chargeSpeed * 1.5f, chargeTelegraphTime * 0.6f));
                yield return StartCoroutine(DoCharge(chargeSpeed * 1.5f, chargeTelegraphTime * 0.6f));
                yield return StartCoroutine(DoRecovery(recoveryTimeFrenzy));
                break;
        }


    }

    IEnumerator DoSummon(int count)
    {
        currentState = BossState.Telegraph;

        StartVisualFeedback(FlashAndPulse(summonTelegraphColor, summonTelegraphTime));
        yield return new WaitForSeconds(summonTelegraphTime);

        currentState = BossState.Summon;

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = soliderSpawnPoints[UnityEngine.Random.Range(0, soliderSpawnPoints.Length)];
            GameObject soldier = Instantiate(soliderPrefab, spawnPoint.position, Quaternion.identity);
            currentSoldierCount++;

            OrangeSolider orangeSolider = soldier.GetComponent<OrangeSolider>();
            if (orangeSolider != null)
            {
                orangeSolider.SetTarget(player);
                orangeSolider.OnDied += () => currentSoldierCount--;
            }
        }

        yield return new WaitForSeconds(0.3f);
        currentState = BossState.Idle;
    }
    IEnumerator DoCharge(float speed, float chargeTime)
    {
        currentState = BossState.Telegraph;
    
        StartVisualFeedback(FlashAndPulse(chargeTelegraphColor, chargeTelegraphTime));

        Vector2 chargeDirection = Vector2.zero;
        float t = 0f;
        while (t < chargeTelegraphTime)
        {
            chargeDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
  
            t += Time.deltaTime;
            yield return null;
        }

        currentState = BossState.Charge;
        if (animator != null) animator.SetTrigger(rollingTrigger); 

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        float elapsed = 0f;
        bool hitWall = false;

        while (elapsed < chargeMaxDuration && !hitWall)
        {
            rb.linearVelocity = chargeDirection * speed;

 
            RaycastHit2D wallCheck = Physics2D.Raycast(transform.position, chargeDirection, 0.6f, groundLayer);
            if (wallCheck.collider != null)
                hitWall = true;

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        currentState = BossState.Idle;

    }
    IEnumerator DoRecovery(float recoveryTime)
    {
        currentState = BossState.Recovery;
   
        StartVisualFeedback(FlashAndPulse(recoveryColor,recoveryTime));
        bossHealth.SetVulnerable(true);

        yield return new WaitForSeconds(recoveryTime);

        bossHealth.SetVulnerable(false);
        currentState = BossState.Idle;
    }

    IEnumerator PlayerAttackWindow()
    {
        currentState = BossState.AttackWindow;
        yield return new WaitForSeconds(attackWindowTime);
        currentState = BossState.Idle;
    }


    void HandleDamaged(float currentHpPercent)
    {
        BossPhase newPhase = currentPhase;

        if (currentHpPercent <= frenzyThreshold)
            newPhase = BossPhase.Frenzy;
        else if (currentHpPercent <= phase3Threshold)
            newPhase = BossPhase.Phase3;
        else if (currentHpPercent <= phase2Threshold)
            newPhase = BossPhase.Phase2;
        else
            newPhase = BossPhase.Phase1;

        if (newPhase != currentPhase)
        {
            StartCoroutine(TransitionToPhase(newPhase));
        }
    }

    IEnumerator TransitionToPhase(BossPhase newPhase)
    {
        if (fightRoutine != null) StopCoroutine(fightRoutine);
        currentState = BossState.Transitioning;


        StartVisualFeedback(FlashAndPulse(phaseTransitionColor, phaseTransitionDuration, pulseMultiplier: 1.5f));
        if (phaseTransitionVFX != null)
            Instantiate(phaseTransitionVFX, transform.position, Quaternion.identity);

        // TODO: Screen Shake

        yield return new WaitForSeconds(phaseTransitionDuration);

        currentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);

        currentState = BossState.Idle;
        fightRoutine = StartCoroutine(FightLoop());
    }

    void HandleDied()
    {
        if (isDead) return;
        isDead = true;

        if (fightRoutine != null) StopCoroutine(fightRoutine);
        if (visualFeedbackRoutine != null) StopCoroutine(visualFeedbackRoutine);
        StopAllCoroutines();

        currentState = BossState.Dead;
        currentPhase = BossPhase.Dead;

        if (spriteRenderer != null) spriteRenderer.color = baseColor;
        transform.localScale = baseScale;

        GetComponent<Collider2D>().enabled = false;
        if (animator != null) animator.SetTrigger(deathTrigger);

        OnBossDied?.Invoke();
    }


    void StartVisualFeedback(IEnumerator routine)
    {
        if (visualFeedbackRoutine != null)
            StopCoroutine(visualFeedbackRoutine);
        visualFeedbackRoutine = StartCoroutine(routine);
    }

    IEnumerator FlashAndPulse(Color targetColor, float duration, float pulseMultiplier = 1f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = targetColor;

            float pulse = 1f + Mathf.Sin(elapsed * pulseSpeed) * pulseScaleAmount * pulseMultiplier;
            transform.localScale = baseScale * pulse;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;
        transform.localScale = baseScale;
        visualFeedbackRoutine = null;
    }
}
