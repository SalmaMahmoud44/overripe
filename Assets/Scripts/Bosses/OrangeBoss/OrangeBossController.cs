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
    [SerializeField] Rigidbody2D orangeRigidbody2D;

    [Header("Animator")]
    [SerializeField] string rollingTrigger = "Rolling";
    [SerializeField] string deathTrigger = "Death";

    [Header("Flip")]
    [SerializeField] bool spriteFacesLeftByDefault = false;

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

    [SerializeField] float wallCheckDistance = 0.6f;
    [SerializeField] LayerMask groundLayer;

    [Header("Charge Damage")]
    [SerializeField] float chargeDamage = 10f;

    [Header("Charge Knockback")]
    [SerializeField] float chargeKnockbackForce = 12f;
    [SerializeField] float chargeKnockbackUpwardForce = 4f;

    [Header("Wall Stun")]
    [SerializeField] float wallStunTimeP1 = 1.2f;
    [SerializeField] float wallStunTimeP2 = 1f;
    [SerializeField] float wallStunTimeP3 = 0.8f;
    [SerializeField] float wallStunTimeFrenzy = 0.7f;

    [Header("Summon Settings")]
    [SerializeField] int solidersToSummonP1 = 2;
    [SerializeField] int soldiersToSummonP2 = 3;
    [SerializeField] int soldiersToSummonP3 = 3;

    [SerializeField] float summonTelegraphTime = 0.6f;
    [SerializeField] float delayAfterSummon = 1.5f;

    [Header("Summon Cooldown")]
    [SerializeField] float summonCooldownP1 = 8f;
    [SerializeField] float summonCooldownP2 = 7f;
    [SerializeField] float summonCooldownP3 = 6f;
    [SerializeField] float summonCooldownFrenzy = 5f;

    [Header("Soldier Limits")]
    [SerializeField] int maxSoldiersP1 = 2;
    [SerializeField] int maxSoldiersP2 = 3;
    [SerializeField] int maxSoldiersP3 = 4;
    [SerializeField] int maxSoldiersFrenzy = 5;

    [Header("Recovery Settings")]
    [SerializeField] float recoveryTimeP1 = 1.8f;
    [SerializeField] float recoveryTimeP2 = 1.3f;
    [SerializeField] float recoveryTimeP3 = 1.2f;
    [SerializeField] float recoveryTimeFrenzy = 1f;

    [Header("Breathing Room")]
    [SerializeField] float breathingRoomP1 = 0.8f;
    [SerializeField] float breathingRoomP2 = 0.7f;
    [SerializeField] float breathingRoomP3 = 0.6f;
    [SerializeField] float breathingRoomFrenzy = 0.5f;


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
    bool isTransitioning = false;
    bool hasHitPlayerThisCharge = false;
    bool isCharging =false;

    int currentSoldierCount = 0;
    float summonTimer=0f;


    private void Awake()
    {
        if(bossHealth == null) 
            bossHealth = GetComponent<BossHealth>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if(orangeRigidbody2D == null)
            orangeRigidbody2D = GetComponentInChildren<Rigidbody2D>();

        baseScale = transform.localScale;
        baseColor = spriteRenderer!=null ? spriteRenderer.color : Color.white;
    }

    private void Update()
    {
        if(summonTimer > 0)
            summonTimer -= Time.deltaTime;

        if(isDead || player == null)
            return;

        if (currentState == BossState.Idle ||
            currentState == BossState.Telegraph ||
            currentState == BossState.Summon)
        {
            FacePlayer();
        }
    }

    private void OnEnable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDamaged += HandleDamaged;
            bossHealth.OnDied += HandleDied;
        }
    }
    void OnDisable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDamaged -= HandleDamaged;
            bossHealth.OnDied -= HandleDied;
        }
    }

    public void BeginFight()
    {
        if(isDead) return;
        if (fightRoutine != null) return;

        fightRoutine = StartCoroutine(FightLoop());
    }

    int GetMaxSoliders()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                return maxSoldiersP1;
            case BossPhase.Phase2:
                return maxSoldiersP2;
            case BossPhase.Phase3:
                return maxSoldiersP3;
            case BossPhase.Frenzy:
                return maxSoldiersFrenzy;
            default:
                return 0;
        }
    }

    float GetSummonCooldown()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                return summonCooldownP1;

            case BossPhase.Phase2:
                return summonCooldownP2;

            case BossPhase.Phase3:
                return summonCooldownP3;

            case BossPhase.Frenzy:
                return summonCooldownFrenzy;

            default:
                return 999f;
        }
    }
    float GetRecoveryTime()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                return recoveryTimeP1;

            case BossPhase.Phase2:
                return recoveryTimeP2;

            case BossPhase.Phase3:
                return recoveryTimeP3;

            case BossPhase.Frenzy:
                return recoveryTimeFrenzy;

            default:
                return 0f;
        }
    }

    float GetBreathingRoom()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                return breathingRoomP1;

            case BossPhase.Phase2:
                return breathingRoomP2;

            case BossPhase.Phase3:
                return breathingRoomP3;

            case BossPhase.Frenzy:
                return breathingRoomFrenzy;

            default:
                return 0f;
        }
    }

    float GetWallStunTime()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                return wallStunTimeP1;

            case BossPhase.Phase2:
                return wallStunTimeP2;

            case BossPhase.Phase3:
                return wallStunTimeP3;

            case BossPhase.Frenzy:
                return wallStunTimeFrenzy;

            default:
                return 0f;
        }
    }

    bool CanSummon()
    {
        return summonTimer <= 0f && currentSoldierCount < GetMaxSoliders();
    }

    bool ShouldSummon()
    {
        if(!CanSummon()) return false;
        int maxSoliders = GetMaxSoliders();

        return currentSoldierCount <= Mathf.FloorToInt(maxSoliders*0.5f);
    }

    IEnumerator FightLoop()
    {
        while (!isDead) 
        {
            if (isTransitioning)
            {
                yield return null;
                continue;
            }
            yield return StartCoroutine(RunPattern(currentPhase));

            if(isDead||isTransitioning)
                yield break;

            yield return new WaitForSeconds(GetBreathingRoom());
        }

        fightRoutine = null;
    }

    IEnumerator RunPattern(BossPhase phase)
    {
        switch (phase) 
        { 
            case BossPhase.Phase1:
                if (ShouldSummon())
                {
                    yield return StartCoroutine(DoSummon(solidersToSummonP1));
                    yield return new WaitForSeconds(delayAfterSummon);
                } 
                yield return StartCoroutine(DoCharge(chargeSpeed, chargeTelegraphTime));
                yield return StartCoroutine(DoRecovery(GetRecoveryTime()));
                break;
            case BossPhase.Phase2:

                float randomVal = UnityEngine.Random.value;
                if (randomVal < 0.33f)
                {
                    if (ShouldSummon())
                    {
                        yield return StartCoroutine(DoSummon(soldiersToSummonP2));
                        yield return new WaitForSeconds(delayAfterSummon);
                    }
                    yield return StartCoroutine(DoCharge(chargeSpeed * 1.15f, chargeTelegraphTime * 0.9f));
                }
                else if (randomVal <0.66f)
                {
                    yield return StartCoroutine(DoCharge(chargeSpeed * 1.15f, chargeTelegraphTime * 0.9f));
                    if (ShouldSummon())
                    {
                        yield return new WaitForSeconds(0.5f);
                        yield return StartCoroutine(DoSummon(soldiersToSummonP2));
                    }
                }
                else
                {
                    yield return StartCoroutine( DoCharge(chargeSpeed * 1.1f,chargeTelegraphTime * 0.9f));
                    yield return new WaitForSeconds(0.6f);
                    yield return StartCoroutine(DoCharge(chargeSpeed * 1.1f,chargeTelegraphTime * 0.9f));
                }

                yield return StartCoroutine(DoRecovery(GetRecoveryTime()));
                break;
            case BossPhase.Phase3:
                 randomVal = UnityEngine.Random.value;
                if (randomVal <0.25f && ShouldSummon())
                {
                    yield return StartCoroutine(DoSummon(soldiersToSummonP3));
                    yield return new WaitForSeconds(delayAfterSummon);
                    yield return StartCoroutine(DoCharge(chargeSpeed * 1.2f, chargeTelegraphTime * 0.8f));
                }
                else
                {
                    yield return StartCoroutine(DoCharge(chargeSpeed * 1.2f, chargeTelegraphTime * 0.8f));
                    yield return new WaitForSeconds(0.55f);
                    yield return StartCoroutine(DoCharge(chargeSpeed * 1.2f, chargeTelegraphTime * 0.8f));

                }
                yield return StartCoroutine(DoRecovery(GetRecoveryTime()));
                break;

            case BossPhase.Frenzy:
                randomVal = UnityEngine.Random.value;
                if (ShouldSummon() && randomVal<0.4f )
                {
                    yield return StartCoroutine(DoSummon(soldiersToSummonP3));
                    yield return new WaitForSeconds(1f);

                }   
                yield return StartCoroutine(DoCharge(chargeSpeed * 1.5f, chargeTelegraphTime * 0.6f));
                yield return new WaitForSeconds(0.4f);
                yield return StartCoroutine(DoCharge(chargeSpeed * 1.5f, chargeTelegraphTime * 0.6f));
                yield return StartCoroutine(DoRecovery(GetRecoveryTime()));
                break;
        }


    }

    IEnumerator DoSummon(int count)
    {
        if (!CanSummon()) 
        { 
            currentState = BossState.Idle;
            yield break;
        }

        currentState = BossState.Telegraph;

        FacePlayer()
;
        StartVisualFeedback(FlashAndPulse(summonTelegraphColor, summonTelegraphTime));
        yield return new WaitForSeconds(summonTelegraphTime);

        if (isDead || isTransitioning)
            yield break;

        currentState = BossState.Summon;

        int maxSoliders = GetMaxSoliders();
        int avaliableSlots = maxSoliders - currentSoldierCount;
        int spawnCount = Mathf.Min(count,avaliableSlots);
        
        if (spawnCount <= 0)
        {
            currentState = BossState.Idle;
            yield break;
        }

        summonTimer = GetSummonCooldown();

        for (int i = 0; i < spawnCount; i++)
        {
            if (soliderSpawnPoints == null || soliderSpawnPoints.Length == 0)
                   break;

            Transform spawnPoint = soliderSpawnPoints[UnityEngine.Random.Range(0, soliderSpawnPoints.Length )];
            
            GameObject soldier = Instantiate(soliderPrefab,spawnPoint.position,Quaternion.identity);

            currentSoldierCount++;

            OrangeSolider orangeSolider =soldier.GetComponent<OrangeSolider>();

            if (orangeSolider != null)
            {
                orangeSolider.SetTarget(player);

                orangeSolider.OnDied += HandleSoldierDied;
            }

            yield return new WaitForSeconds(0.3f);
        }

        currentState = BossState.Idle;
    }

    void HandleSoldierDied()
    {
        currentSoldierCount = Mathf.Max( 0,currentSoldierCount - 1);
    }

    IEnumerator DoCharge(float speed, float telegraphTime)
    {
        if (player == null)
            yield break;

        currentState = BossState.Telegraph;

        FacePlayer();

        StartVisualFeedback( FlashAndPulse(chargeTelegraphColor,telegraphTime));

        Vector2 chargeDirection = Vector2.zero;

        float timer = 0f;

        while (timer < telegraphTime)
        {
            if (player == null)
                yield break;

            chargeDirection =((Vector2)player.position -(Vector2)transform.position).normalized;

            FaceDirection(chargeDirection.x);

            timer += Time.deltaTime;

            yield return null;
        }

        if (chargeDirection == Vector2.zero)
            chargeDirection = Vector2.right;

        currentState = BossState.Charge;

        isCharging = true;
        hasHitPlayerThisCharge = false;

        FaceDirection(chargeDirection.x);

        if (animator != null)
            animator.SetTrigger(rollingTrigger);

        float elapsed = 0f;
        bool hitWall = false;

        while (elapsed < chargeMaxDuration)
        {
            if (isDead || isTransitioning)
                yield break;

            orangeRigidbody2D.linearVelocity = chargeDirection * speed;

            RaycastHit2D wallCheck =Physics2D.Raycast(transform.position,chargeDirection, wallCheckDistance,groundLayer );

            if (wallCheck.collider != null)
            {
                hitWall = true;
                break;
            }

            elapsed += Time.deltaTime;

            yield return null;
        }

        orangeRigidbody2D.linearVelocity = Vector2.zero;

        isCharging = false; 
        hasHitPlayerThisCharge = false;

        if (hitWall)
        {
            yield return StartCoroutine( WallStun() );
        }
        else
        {
            currentState = BossState.Idle;
        }
    }


    IEnumerator WallStun()
    {
        currentState = BossState.Recovery;

        orangeRigidbody2D.linearVelocity = Vector2.zero;

        StartVisualFeedback(
            FlashAndPulse(
                recoveryColor,
                GetWallStunTime(),
                1.2f
            )
        );

        bossHealth.SetVulnerable(true);

        yield return new WaitForSeconds(GetWallStunTime() );

        bossHealth.SetVulnerable(false);

        currentState = BossState.Idle;
    }



    IEnumerator DoRecovery(float recoveryTime)
    {
        currentState = BossState.Recovery;

        orangeRigidbody2D.linearVelocity = Vector2.zero;

        StartVisualFeedback(FlashAndPulse(recoveryColor,recoveryTime ));


        bossHealth.SetVulnerable(true);

        yield return new WaitForSeconds(
            recoveryTime
        );

        bossHealth.SetVulnerable(false);

        currentState = BossState.Idle;
    }


    void FacePlayer()
    {
        if (player == null)
            return;

        float directionX = player.position.x - transform.position.x;

        if (Mathf.Abs(directionX) < 0.01f)
            return;

        FaceDirection(directionX);
    }


    void FaceDirection(float directionX)
    {
        if (spriteRenderer == null)
            return;

        if (Mathf.Abs(directionX) < 0.01f)
            return;

        bool lookingRight = directionX > 0f;

        if (spriteFacesLeftByDefault)
        {
            spriteRenderer.flipX = lookingRight;
        }
        else
        {
            spriteRenderer.flipX = !lookingRight;
        }
    }
    void HandleDamaged(float currentHpPercent)
    {
        if(isDead||isTransitioning)
            { return; }

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
        if (isDead || isTransitioning)
            yield break;

        isTransitioning = true;

        if (fightRoutine != null)
        {
            StopCoroutine(fightRoutine);
            fightRoutine = null;
        }

        orangeRigidbody2D.linearVelocity = Vector3.zero;
        currentState = BossState.Transitioning;


        StartVisualFeedback(FlashAndPulse(phaseTransitionColor, phaseTransitionDuration, pulseMultiplier: 1.5f));
        if (phaseTransitionVFX != null)
            Instantiate(phaseTransitionVFX, transform.position, Quaternion.identity);

        // TODO: Screen Shake

        yield return new WaitForSeconds(phaseTransitionDuration);

        if (isDead)
            yield break;

        currentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);

        currentState = BossState.Idle;

        yield return new WaitForSeconds(0.5f);
        
        isTransitioning = false;

        if (!isDead)
        {
            fightRoutine = StartCoroutine(FightLoop());
        }
  
    }

    void HandleDied()
    {
        if (isDead)
            return;

        isDead = true;
        isTransitioning = false;

        if (fightRoutine != null)
        {
            StopCoroutine(fightRoutine);
            fightRoutine = null;
        }

        if (visualFeedbackRoutine != null)
        {
            StopCoroutine(visualFeedbackRoutine);
            visualFeedbackRoutine = null;
        }

        orangeRigidbody2D.linearVelocity = Vector2.zero;

        currentState = BossState.Dead;
        currentPhase = BossPhase.Dead;

        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;

        transform.localScale = baseScale;

        Collider2D col =GetComponent<Collider2D>();

        if (col != null)
            col.enabled = false;

        if (animator != null)
            animator.SetTrigger(deathTrigger);

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!isCharging||isDead) return;

        if(hasHitPlayerThisCharge)  return;

        PlayerDeath playerDeath = collision.collider.GetComponent<PlayerDeath>();

        if(playerDeath == null)
            return;

        hasHitPlayerThisCharge = true;

        playerDeath.TakeDamage(chargeDamage);

        KnockBack knockBack = playerDeath.GetComponent<KnockBack>();

        if(knockBack != null)
        {
            Vector2 direction = playerDeath.transform.position - transform.position;

            direction = new Vector2(Mathf.Sign(direction.x), 0f);

            knockBack.ApplyKnockback(direction, chargeKnockbackForce, chargeKnockbackUpwardForce);
        }
    }
}
