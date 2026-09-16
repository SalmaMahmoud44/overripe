using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;
using UnityEngine;
using Unity.Cinemachine;

public class OrangeBossController : MonoBehaviour, ILaserStunnable
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
    [SerializeField] CinemachineImpulseSource impulseSource;

    [Header("Animator")]
    [SerializeField] string rollingTrigger = "Rolling";
    [SerializeField] string deathTrigger = "Death";
    [SerializeField] string idleStateName = "Idle";

    [Header("Hit Reaction")]
    [SerializeField] string hitTrigger = "Hit";
    [SerializeField] string hitStateName = "OrangeHit";
    [SerializeField] float normalHitCooldown = 0.12f;
    [SerializeField] float normalHitDuration = 0.12f;


    [Header("Flip")]
    [SerializeField] bool spriteFacesLeftByDefault = false;


    [Header("Phase Threshold")]
    [SerializeField] float phase2Threshold = 0.7f;
    [SerializeField] float phase3Threshold = 0.4f;
    [SerializeField] float frenzyThreshold = 0.15f;

    [Header("Charge Settings")]
    [SerializeField] float chargeTelegraphTime = 1f;
    [SerializeField] float chargeSpeed = 14f;
    [SerializeField] float chargeMaxDuration = 1.2f;

    [SerializeField] float wallCheckDistance = 0.6f;
    [SerializeField] LayerMask groundLayer;

    [Header("Charge Damage")]
    [SerializeField] float chargeDamage = 10f;

    [Header("Charge Knockback")]
    [SerializeField] float chargeKnockbackForce = 20f;
    [SerializeField] float chargeKnockbackUpwardForce = 7f;

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

    [Header("Soldier Spawn Timing")]
    [SerializeField] float soldierSpawnWarningTime = 0.35f;
    [SerializeField] float delayBetweenSoldiers = 0.2f;

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

    [Header("Polish - Charge")]
    [SerializeField] float telegraphSquashAmount = 0.12f;
    [SerializeField] float telegraphPulseSpeed = 12f;

    [Header("Polish - Hit")]
    [SerializeField] float hitStopTime = 0.04f;
    [SerializeField] float hitSquashAmount = 0.15f;

    [Header("Polish - VFX")]
    [SerializeField] ParticleSystem chargeDust;
    [SerializeField] ParticleSystem impactDust;
    [SerializeField] ParticleSystem hitParticles;

    [Header("Summon VFX")]
    [SerializeField] ParticleSystem summonEffect;
    [SerializeField] ParticleSystem soldierSpawnEffect;

    [Header("Phase Transition")]
    [SerializeField] float phaseTransitionDuration = 0.8f;
    [SerializeField] float enrageDuration = 0.45f;

    [Header("Enrage Polish")]
    [SerializeField] float enrageSquashAmount = 0.18f;
    [SerializeField] float enragePulseSpeed = 18f;
    [SerializeField] float enrageScalePunch = 1.12f;

    [Header("Phase Charge Boost")]
    [SerializeField] float phase2FirstChargeMultiplier = 1.25f;
    [SerializeField] float phase3FirstChargeMultiplier = 1.35f;
    [SerializeField] float frenzyFirstChargeMultiplier = 1.5f;

    [Header("Death Reward")]
    [SerializeField] GameObject artifactPrefab;
    [SerializeField] float artifactSpawnDelay = 0.5f;


    public event Action<BossPhase> OnPhaseChanged;
    public event Action OnBossDied;

    public BossPhase currentPhase { get; private set; } = BossPhase.Phase1;
    public BossState currentState { get; private set; } = BossState.Idle;

    public bool IsLaserStunned => isLaserStunned;


    Coroutine fightRoutine;
    Coroutine transitionRoutine;
    Coroutine squashRoutine;

    Vector3 baseScale;
    Color baseColor;

    bool isDead = false;
    bool isTransitioning = false;
    bool hasHitPlayerThisCharge = false;
    bool isCharging =false;
    bool isFirstChargeAfterPhase = false;
    bool isLaserStunned = false;
    bool cancelCurrentCharge = false;

  

    int currentSoldierCount = 0;
    float summonTimer = 0f;
    float normalHitTimer = 0f;


    private void Awake()
    {
        if(bossHealth == null) 
            bossHealth = GetComponent<BossHealth>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if(orangeRigidbody2D == null)
            orangeRigidbody2D = GetComponent<Rigidbody2D>();

        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>();

        baseScale = transform.localScale;

        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    private void Update()
    {
        if(summonTimer > 0)
            summonTimer -= Time.deltaTime;

        if (normalHitTimer > 0f)
            normalHitTimer -= Time.deltaTime;

        if (isDead || player == null)
            return;

        if (isLaserStunned)
            return;

        if (currentState == BossState.Idle ||currentState == BossState.Telegraph ||currentState == BossState.Summon)
        {
            FacePlayer();
        }
    }

    private void OnEnable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDamaged += HandleDamaged;
            bossHealth.OnHit += HandleBossHit;
            bossHealth.OnDied += HandleDied;
        }
    }
    void OnDisable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDamaged -= HandleDamaged;
            bossHealth.OnHit -= HandleBossHit;
            bossHealth.OnDied -= HandleDied;
        }
    }

    public void BeginFight()
    {
        if(isDead) return;
        if (fightRoutine != null) return;

        fightRoutine = StartCoroutine(FightLoop());
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
                else if (randomVal < 0.66f)
                {
                    yield return StartCoroutine(DoCharge(chargeSpeed * 1.15f,chargeTelegraphTime * 0.9f));

                    yield return StartCoroutine(DoRecovery(GetRecoveryTime()));

                    if (ShouldSummon())
                    {
                        yield return StartCoroutine(DoSummon(soldiersToSummonP2));

                        yield return new WaitForSeconds(1.5f);
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

        if (isDead || isTransitioning || isLaserStunned)
        {
            currentState = BossState.Idle;
            yield break;
        }

        if (!CanSummon())
        {
            currentState = BossState.Idle;
            yield break;
        }

        currentState = BossState.Telegraph;

        FacePlayer();

        if (summonEffect != null)
        {
            summonEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            summonEffect.Play();
        }

        float timer = 0f;

        while (timer < summonTelegraphTime)
        {
            if (isDead || isTransitioning||isLaserStunned)
            {
                transform.localScale = baseScale;
                currentState = BossState.Idle;
                yield break;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.06f;

            transform.localScale = new Vector3(baseScale.x * (2f - pulse),baseScale.y * pulse,baseScale.z);

            timer += Time.deltaTime;

            yield return null;
        }

        transform.localScale = baseScale;

        if (isDead || isTransitioning)
            yield break;

        currentState = BossState.Summon;

        int maxSoldiers = GetMaxSoliders();
        int availableSlots = maxSoldiers - currentSoldierCount;
        int spawnCount = Mathf.Min(count, availableSlots);

        if (spawnCount <= 0)
        {
            currentState = BossState.Idle;
            yield break;
        }

        summonTimer = GetSummonCooldown();

        for (int i = 0; i < spawnCount; i++)
        {
            if (isDead || isTransitioning || isLaserStunned)
            {
                transform.localScale = baseScale;
                currentState = BossState.Idle;
                yield break;
            }

            if (soliderSpawnPoints == null || soliderSpawnPoints.Length == 0)
                break;

            Transform spawnPoint =soliderSpawnPoints[UnityEngine.Random.Range(0, soliderSpawnPoints.Length)];


            if (soldierSpawnEffect != null)
            {
                soldierSpawnEffect.transform.position = spawnPoint.position;

                soldierSpawnEffect.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);

                soldierSpawnEffect.Play();
            }

            yield return new WaitForSeconds(soldierSpawnWarningTime);

            if (isDead || isTransitioning || isLaserStunned)
            {
                transform.localScale = baseScale;
                currentState = BossState.Idle;
                yield break;
            }

            GameObject soldier = Instantiate(soliderPrefab,spawnPoint.position,Quaternion.identity);

            currentSoldierCount++;

            OrangeSolider orangeSolider =soldier.GetComponent<OrangeSolider>();

            if (orangeSolider != null)
            {
                orangeSolider.SetTarget(player);
                orangeSolider.OnDied += HandleSoldierDied;
            }

            yield return new WaitForSeconds(delayBetweenSoldiers);
        }

        currentState = BossState.Idle;
    }

    void HandleSoldierDied()
    {
        currentSoldierCount = Mathf.Max( 0,currentSoldierCount - 1);
    }

    IEnumerator DoCharge(float speed, float telegraphTime)
    {
        if (player == null || isDead || isTransitioning)
            yield break;

        cancelCurrentCharge = false; ;

        if (isFirstChargeAfterPhase)
        {
            speed *= GetFirstChargeMultiplier();
            isFirstChargeAfterPhase = false;
        }

        currentState = BossState.Telegraph;

        FacePlayer();

        float timer = 0f;

        while (timer < telegraphTime)
        {
            if (isDead || isTransitioning)
                yield break;

            if (isLaserStunned)
            {
                yield return null;
                continue;
            }
            float pulse = 1f + Mathf.Sin(Time.time * telegraphPulseSpeed) * telegraphSquashAmount;

            transform.localScale = new Vector3(baseScale.x * (2f - pulse),baseScale.y * pulse,baseScale.z);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = baseScale;


        float differenceX = player.position.x - transform.position.x;

        float directionX;

        if (Mathf.Abs(differenceX) > 0.05f)
        {

            directionX = Mathf.Sign(differenceX);
        }
        else
        {
            directionX =
                spriteRenderer != null && !spriteRenderer.flipX? 1f: -1f;
        }

        Vector2 chargeDirection = new Vector2(directionX, 0f);


        FaceDirection(directionX);

        currentState = BossState.Charge;

        isCharging = true;
        hasHitPlayerThisCharge = false;



        float oldGravityScale = orangeRigidbody2D.gravityScale;
        RigidbodyConstraints2D oldConstraints = orangeRigidbody2D.constraints;


        orangeRigidbody2D.gravityScale = 0f;

        orangeRigidbody2D.constraints = oldConstraints | RigidbodyConstraints2D.FreezePositionY;


        if (animator != null)
        {
            animator.ResetTrigger(rollingTrigger);
            animator.SetTrigger(rollingTrigger);
        }

        if (chargeDust != null)
            chargeDust.Play();

     

        float elapsed = 0f;
        bool hitWall = false;

        while (elapsed < chargeMaxDuration)
        {
            if (isDead || isTransitioning)
                yield break;

            if (isLaserStunned)
            {
                orangeRigidbody2D.linearVelocity = Vector2.zero;

                while (isLaserStunned && !isDead)
                {
                    yield return null;
                }

                if (isDead)
                    yield break;

                cancelCurrentCharge = true;

                break;
            }

            orangeRigidbody2D.linearVelocity = new Vector2(directionX * speed, 0f);

            RaycastHit2D wallCheck = Physics2D.Raycast(transform.position,chargeDirection,wallCheckDistance,groundLayer);

            if (wallCheck.collider != null)
            {
                hitWall = true;
                break;
            }

            elapsed += Time.deltaTime;

            yield return null;
        }


        orangeRigidbody2D.linearVelocity = Vector2.zero;


        orangeRigidbody2D.gravityScale = oldGravityScale;
        orangeRigidbody2D.constraints = oldConstraints;

        isCharging = false;
        hasHitPlayerThisCharge = false;

        if (chargeDust != null)
            chargeDust.Stop();

        PlayIdleAnimation();

        if (cancelCurrentCharge)
        {
            cancelCurrentCharge = false;
            currentState = BossState.Idle;
            yield break;
        }

        if (hitWall)
        {
            yield return StartCoroutine(WallStun());
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

        if (impactDust != null)
        {
            float wallDirection = spriteRenderer != null && spriteRenderer.flipX? -1f: 1f;

            impactDust.transform.position =transform.position + new Vector3(wallDirection * 0.5f, 0f, 0f);

            impactDust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);

            impactDust.Play();
        }

        CameraShake(0.12f);

        StartSquash(new Vector3(baseScale.x * 1.15f, baseScale.y * 0.75f, baseScale.z), 0.12f);
     

        bossHealth.SetVulnerable(true);

        yield return new WaitForSeconds(GetWallStunTime());

        bossHealth.SetVulnerable(false);

        currentState = BossState.Idle;

        transform.localScale = baseScale;
    }



    IEnumerator DoRecovery(float recoveryTime)
    {
        currentState = BossState.Recovery;

        orangeRigidbody2D.linearVelocity = Vector2.zero;

        PlayIdleAnimation();

        bossHealth.SetVulnerable(true);

        yield return new WaitForSeconds(recoveryTime);

        bossHealth.SetVulnerable(false);

        currentState = BossState.Idle;
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
        if (!CanSummon()) return false;
        int maxSoliders = GetMaxSoliders();

        return currentSoldierCount <= Mathf.FloorToInt(maxSoliders * 0.5f);
    }

    void PlayIdleAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(rollingTrigger);

        animator.Play(idleStateName);
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
            if (transitionRoutine != null)
                StopCoroutine(transitionRoutine);

            transitionRoutine =StartCoroutine(TransitionToPhase(newPhase));
        }
    }

    void CameraShake(float force)
    {
        if (impulseSource == null)
            return;

        impulseSource.GenerateImpulse(force);
    }

    float GetPhaseShakeForce(BossPhase phase)
    {
        switch (phase)
        {
            case BossPhase.Phase2:
                return 0.3f;

            case BossPhase.Phase3:
                return 0.4f;

            case BossPhase.Frenzy:
                return 0.5f;

            default:
                return 0.25f;
        }
    }
    float GetFirstChargeMultiplier()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase2:
                return phase2FirstChargeMultiplier;

            case BossPhase.Phase3:
                return phase3FirstChargeMultiplier;

            case BossPhase.Frenzy:
                return frenzyFirstChargeMultiplier;

            default:
                return 1f;
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


        while (isLaserStunned && !isDead)
            yield return null;

        if (isDead)
            yield break;

        orangeRigidbody2D.linearVelocity = Vector3.zero;
        currentState = BossState.Transitioning;

        PlayIdleAnimation();


        yield return StartCoroutine(EnrageEffect());

        CameraShake(GetPhaseShakeForce(newPhase));

        currentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);

        isFirstChargeAfterPhase = true;

        currentState = BossState.Idle;

        yield return new WaitForSeconds(0.2f);

        isTransitioning = false;

        if (!isDead)
        {
            fightRoutine = StartCoroutine(FightLoop());
        }

        transitionRoutine = null;

    }
    private void HandleBossHit()
    {
        PlayNormalHitReaction();
    }
    void HandleDied()
    {
        if (isDead)
            return;

        isDead = true;
        isTransitioning = false;

        if (animator != null)
        {
            animator.ResetTrigger(rollingTrigger);
            animator.ResetTrigger(hitTrigger);

            animator.speed = 1f;
            animator.SetTrigger(deathTrigger);
        }

        if (fightRoutine != null)
        {
            StopCoroutine(fightRoutine);
            fightRoutine = null;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (squashRoutine != null)
        {
            StopCoroutine(squashRoutine);
            squashRoutine = null;
        }

        orangeRigidbody2D.linearVelocity = Vector2.zero;

        if (chargeDust != null)
            chargeDust.Stop();

        currentState = BossState.Dead;
        currentPhase = BossPhase.Dead;

        transform.localScale = baseScale;


        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;

        transform.localScale = baseScale;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.isTrigger = false;
            orangeRigidbody2D.bodyType = RigidbodyType2D.Static;
        }

        OnBossDied?.Invoke();
        CameraShake(0.3f);

        StartCoroutine(SpawnArtifactAndDestroy());

    }

    IEnumerator SpawnArtifactAndDestroy()
    {
        Vector3 spawnPosition = transform.position;

        yield return new WaitForSeconds(artifactSpawnDelay);

        if (artifactPrefab != null)
        {
            Instantiate(artifactPrefab,spawnPosition,Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void StartSquash(Vector3 targetScale, float duration)
    {
        if (squashRoutine != null)
            StopCoroutine(squashRoutine);

        squashRoutine = StartCoroutine(SquashRoutine(targetScale, duration));
    }


    IEnumerator SquashRoutine(Vector3 targetScale,float duration)
    {
        Vector3 startScale =transform.localScale;

        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;

            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(startScale, targetScale,t );

            timer += Time.deltaTime;

            yield return null;
        }

        transform.localScale = baseScale;

        squashRoutine = null;
    }

    IEnumerator HitStop()
    {
        float previousTimeScale = Time.timeScale;

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(hitStopTime);

        Time.timeScale = previousTimeScale;
    }
    IEnumerator EnrageEffect()
    {
        float timer = 0f;

        Vector3 startScale = baseScale;

        while (timer < enrageDuration)
        {
            if (isDead)
                yield break;

            float normalizedTime = timer / enrageDuration;

            float pulse =1f + Mathf.Sin(timer * enragePulseSpeed) * enrageSquashAmount;

            float strength = Mathf.Sin(normalizedTime * Mathf.PI);

            float scaleX = startScale.x * (2f - pulse);
            float scaleY = startScale.y * pulse;

            scaleX += startScale.x * enrageScalePunch * strength;

            transform.localScale = new Vector3(scaleX, scaleY, startScale.z);

            timer += Time.unscaledDeltaTime;

            yield return null;
        }

        transform.localScale = baseScale;
    }
    public void PlayNormalHitReaction()
    {
        if (isDead || isTransitioning || isLaserStunned)
            return;

        if (normalHitTimer > 0f)
            return;

        normalHitTimer = normalHitCooldown;

        StartCoroutine(NormalHitReactionRoutine());
    }

    private IEnumerator NormalHitReactionRoutine()
    {
        if (animator == null)
            yield break;

        animator.speed = 1f;

        animator.ResetTrigger(hitTrigger);
        animator.SetTrigger(hitTrigger);

        yield return new WaitForSeconds(normalHitDuration);

        if (!isDead && !isLaserStunned)
        {
            animator.speed = 1f;
            animator.Play(idleStateName, 0, 0f);
        }
    }

    public void StartLaserStun()
    {
        if (isDead || isTransitioning)
            return;

       if(isLaserStunned)
            return ;


        StartCoroutine(LaserStunRoutine());
    }

    private IEnumerator LaserStunRoutine()
    {
        if (isDead || isTransitioning)
            yield break;

        isLaserStunned = true;

        if (orangeRigidbody2D != null)
            orangeRigidbody2D.linearVelocity = Vector2.zero;

        if (chargeDust != null)
            chargeDust.Stop();

        if (animator != null)
        {
            animator.speed = 1f;

            animator.ResetTrigger(rollingTrigger);
            animator.ResetTrigger(hitTrigger);

            animator.Play(hitStateName, 0, 0f);
        }

        while (isLaserStunned)
        {
            if (orangeRigidbody2D != null)
                orangeRigidbody2D.linearVelocity = Vector2.zero;

            yield return null;
        }

        if (isDead)
            yield break;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play(idleStateName, 0, 0f);
        }

        currentState = BossState.Idle;
    }
    public void EndLaserStun()
    {
        if (!isLaserStunned)
            return;

        isLaserStunned = false;

        if (orangeRigidbody2D != null)
            orangeRigidbody2D.linearVelocity = Vector2.zero;

         if (isDead)
            return;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play(idleStateName, 0, 0f);
        }

   
        currentState = BossState.Idle;
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

        if(knockBack != null && playerDeath.IsDead == false)
        {
            float directionX = Mathf.Sign(playerDeath.transform.position.x - transform.position.x);

            if(Mathf.Abs(directionX) <0.01)
            {
                directionX = - Mathf.Sign(orangeRigidbody2D.linearVelocity.x);
            }

            Vector2 knockbackDirection = new Vector2(directionX,0);

            knockBack.ApplyKnockback(knockbackDirection, chargeKnockbackForce, chargeKnockbackUpwardForce);
        }

        if (hitParticles != null)
        {
            hitParticles.transform.position = playerDeath.transform.position;
            hitParticles.Play();
        }


        StartSquash(new Vector3(baseScale.x * 1.12f,baseScale.y * 0.85f,baseScale.z),hitSquashAmount);

        CameraShake(0.1f);

        if (hitStopTime > 0f)
            StartCoroutine(HitStop());

    }
}
