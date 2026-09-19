using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PeachBossController : MonoBehaviour, IDamagable, IBoss
{
    public enum BossState
    {
        Idle, RollIntro, Rolling, RollOutro, DustAttack, PreTransition, Transitioning,
        Phase2Idle, Phase2JumpRise, Phase2JumpFall, Phase2JumpPause, Phase2Critical, Dead
    }
    public enum BossPhase { Phase1, Phase2 }

    [Header("Health Settings")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth;

    [Header("UI")]
    [SerializeField] Slider healthSlider;

    [Header("Camera Shake")]
    [SerializeField] Unity.Cinemachine.CinemachineImpulseSource impulseSource;
    [SerializeField] float rollShakeInterval = 0.1f;

    float rollShakeTimer;

    [Header("Seed Transition Visuals")]
    [SerializeField] SpriteRenderer seedLandingRenderer;

    [Header("Room Walls")]
    [SerializeField] GameObject leftWall;
    [SerializeField] GameObject rightWall;

    [Header("Detection Settings")]
    [SerializeField] float detectRange = 6f;
    Transform player;

    [Header("Colliders")]
    [SerializeField] Collider2D normalCollider;
    [SerializeField] Collider2D rollCollider;
    [SerializeField] Collider2D seedCollider;

    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Animator")]
    [SerializeField] Animator animator;

    [Header("Room Bounds")]
    [SerializeField] Collider2D bossRoomBounds;
    [SerializeField] CinemachineConfiner2D cameraConfiner;
    Collider2D originalBounds;
    bool roomLocked = false;

    [Header("Roll Platform")]
    [SerializeField] GameObject rollPlatform;

    [Header("Roll Settings")]
    [SerializeField] float spottedDelay = 1.5f;
    [SerializeField] float rollIntroDelay = 1f;
    [SerializeField] float rollSpeed = 6f;
    [SerializeField] int rollBounceCount = 2;
    [SerializeField] bool endWithHalfBounce = true;
    [SerializeField] float rollOutroDelay = 0.5f;
    [SerializeField] ParticleSystem chargeDust;

    [Header("Contact Damage")]
    [SerializeField] float rollDamage = 5f;
    [SerializeField] float rollDamageCooldown = 0.5f;
    [SerializeField] float rollKnockbackForce = 8f;
    [SerializeField] float rollKnockbackUpwardForce = 3f;
    float rollDamageTimer;

    BossState currentState = BossState.Idle;
    BossPhase currentPhase = BossPhase.Phase1;
    float rollDirectionX;
    float stateTimer;
    int currentBounceCount;
    bool doingFinalHalfMove = false;
    float halfMoveTargetX;

    [Header("Visuals")]
    [SerializeField] Transform visualRoot;
    [SerializeField] float rollRotationSpeed = 720f;

    [Header("Dust Attack Settings")]
    [SerializeField] GameObject dustPuffPrefab;
    [SerializeField] int dustPuffCount = 4;
    [SerializeField] float dustPuffInterval = 0.5f;
    [SerializeField] Vector2 dustSpawnOffset = Vector2.zero;
    [SerializeField] float dustAttackEndDelay = 1.5f;
    [SerializeField] float extraDelayBeforeRoll = 1f;

    int currentDustPuffCount;
    bool waitingForDustEnd = false;
    bool waitingExtraDelay = false;
    bool playerSpotted = false;

    [Header("Phase Transition")]
    [SerializeField] float phase2Threshold = 0.5f;
    [SerializeField] float transitionDuration = 1.5f;
    [SerializeField] float preTransitionDelay = 0.3f;
    bool phase2Triggered = false;

    [Header("Phase 2 Settings")]
    [SerializeField] float phase2IdleDelay = 1f;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] float jumpRiseDuration = 0.9f;
    [SerializeField] float jumpFallDuration = 0.6f;
    [SerializeField] float jumpPauseDuration = 0.667f;
    [SerializeField] int jumpsPerCycle = 4;
    [SerializeField] float criticalStateDuration = 2f;
    [SerializeField] float pauseBeforeCritical = 1f;

    int currentJumpCount;
    float jumpElapsed;
    Vector3 jumpStartPos;
    Vector3 jumpPeakPos;
    bool isVulnerable = false;

    [Header("Wave Attack Settings")]
    [SerializeField] GameObject wavePrefab;
    [SerializeField] Vector2 waveSpawnOffset = Vector2.zero;

    [Header("Death Settings")]
    [SerializeField] ParticleSystem explosionEffect;
    [SerializeField] float deathAnimDelay = 1f;
    [SerializeField] float delayBeforeExplosion = 0.3f;
    [SerializeField] GameObject artifactToReveal;

    [Header("Shield UI")]
    [SerializeField] private PeachBossShieldUI shieldUI;


    public event System.Action OnBossDied;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            healthSlider.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (rollDamageTimer > 0f)
            rollDamageTimer -= Time.deltaTime;

        switch (currentState)
        {
            case BossState.Idle:
                UpdateIdle();
                break;

            case BossState.RollIntro:
                UpdateRollIntro();
                break;

            case BossState.Rolling:
                MoveAndBounce();
                break;

            case BossState.RollOutro:
                UpdateRollOutro();
                break;

            case BossState.DustAttack:
                UpdateDustAttack();
                break;

            case BossState.PreTransition:
                UpdatePreTransition();
                break;

            case BossState.Transitioning:
                UpdateTransitioning();
                break;

            case BossState.Phase2Idle:
                UpdatePhase2Idle();
                break;

            case BossState.Phase2JumpRise:
                UpdateJumpRise();
                break;

            case BossState.Phase2JumpFall:
                UpdateJumpFall();
                break;

            case BossState.Phase2JumpPause:
                UpdateJumpPause();
                break;

            case BossState.Phase2Critical:
                UpdateCriticalState();
                break;
        }
    }

    void UpdateIdle()
    {
        if (!playerSpotted)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectRange)
            {
                playerSpotted = true;
                stateTimer = spottedDelay;

                if (healthSlider != null)
                    healthSlider.gameObject.SetActive(true);

                LockCameraToBossRoom();
            }
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            StartRollIntro();
        }
    }

    void SetRollCollider(bool isRolling)
    {
        if (normalCollider != null)
            normalCollider.enabled = !isRolling;

        if (rollCollider != null)
            rollCollider.enabled = isRolling;
    }

    void StartRollIntro()
    {
        currentState = BossState.RollIntro;
        stateTimer = rollIntroDelay;

        if (rollPlatform != null)
            rollPlatform.SetActive(true);

        if (animator != null)
            animator.SetTrigger("RollIntro");
    }

    void UpdateRollIntro()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            StartRolling();
        }
    }

    void StartRolling()
    {
        currentState = BossState.Rolling;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.rollClip);

        rollDirectionX = player.position.x > transform.position.x ? 1f : -1f;
        currentBounceCount = 0;
        doingFinalHalfMove = false;

        SetRollCollider(true);

        if (chargeDust != null)
            chargeDust.Play();

        rollShakeTimer = 0f;
    }

    void StartRollOutro()
    {
        currentState = BossState.RollOutro;
        stateTimer = rollOutroDelay;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        SetRollCollider(false);

        if (rollPlatform != null)
            rollPlatform.SetActive(false);

        FaceDirection(rollDirectionX);

        if (visualRoot != null)
            visualRoot.rotation = Quaternion.identity;

        if (chargeDust != null)
            chargeDust.Stop();

        if (animator != null)
            animator.SetTrigger("RollOutro");
    }

    void UpdateRollOutro()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (phase2Triggered)
            {
                StartTransitioning();
            }
            else
            {
                StartDustAttack();
            }
        }
    }

    void StartDustAttack()
    {
        currentState = BossState.DustAttack;
        currentDustPuffCount = 0;
        waitingForDustEnd = false;
        waitingExtraDelay = false;
        stateTimer = dustPuffInterval;

        float directionX = player.position.x > transform.position.x ? 1f : -1f;
        FaceDirection(directionX);

        if (animator != null)
            animator.SetTrigger("DustAttack");

        SpawnDustPuff();
    }

    void UpdateDustAttack()
    {
        float directionX = player.position.x > transform.position.x ? 1f : -1f;
        FaceDirection(directionX);

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (waitingExtraDelay)
            {
                if (phase2Triggered)
                {
                    StartTransitioning();
                }
                else
                {
                    StartRollIntro();
                }
                return;
            }

            if (waitingForDustEnd)
            {
                waitingExtraDelay = true;
                stateTimer = extraDelayBeforeRoll;
                return;
            }

            currentDustPuffCount++;

            SpawnDustPuff();

            if (currentDustPuffCount >= dustPuffCount)
            {
                if (animator != null)
                    animator.SetTrigger("DustAttackEnd");

                waitingForDustEnd = true;
                stateTimer = dustAttackEndDelay;
                return;
            }

            stateTimer = dustPuffInterval;
        }
    }

    void SpawnDustPuff()
    {
        if (dustPuffPrefab == null || player == null)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.dustPuffClip);

        float directionX = player.position.x > transform.position.x ? 1f : -1f;
        Vector2 direction = new Vector2(directionX, 0f);

        Vector3 spawnPos = transform.position + new Vector3(dustSpawnOffset.x * directionX, dustSpawnOffset.y, 0f);

        GameObject puff = Instantiate(dustPuffPrefab, spawnPos, Quaternion.identity);

        DustPuff puffScript = puff.GetComponent<DustPuff>();
        if (puffScript != null)
            puffScript.Init(direction, bossRoomBounds);
    }

    void MoveAndBounce()
    {
        rb.linearVelocity = new Vector2(rollDirectionX * rollSpeed, rb.linearVelocity.y);

        if (visualRoot != null)
        {
            visualRoot.Rotate(0f, 0f, -rollDirectionX * rollRotationSpeed * Time.deltaTime);
        }

        rollShakeTimer -= Time.deltaTime;
        if (rollShakeTimer <= 0f)
        {
            if (impulseSource != null)
                impulseSource.GenerateImpulse();

            rollShakeTimer = rollShakeInterval;
        }

        if (bossRoomBounds == null)
            return;

        float minX = bossRoomBounds.bounds.min.x;
        float maxX = bossRoomBounds.bounds.max.x;

        if (doingFinalHalfMove)
        {
            bool reachedTarget = rollDirectionX > 0f
                ? transform.position.x >= halfMoveTargetX
                : transform.position.x <= halfMoveTargetX;

            if (reachedTarget)
            {
                StartRollOutro();
            }
            return;
        }

        if (transform.position.x <= minX && rollDirectionX < 0f)
        {
            rollDirectionX = 1f;
            UpdateFlip();
            RegisterBounce(minX, maxX);
        }
        else if (transform.position.x >= maxX && rollDirectionX > 0f)
        {
            rollDirectionX = -1f;
            UpdateFlip();
            RegisterBounce(minX, maxX);
        }
    }

    void RegisterBounce(float minX, float maxX)
    {
        currentBounceCount++;

        if (currentBounceCount >= rollBounceCount)
        {
            if (endWithHalfBounce)
            {
                doingFinalHalfMove = true;
                halfMoveTargetX = (minX + maxX) / 2f;
            }
            else
            {
                StartRollOutro();
            }
        }
    }

    void UpdateFlip()
    {
        FaceDirection(rollDirectionX);
    }

    void FaceDirection(float directionX)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.flipX = directionX > 0f;
    }

    void LockCameraToBossRoom()
    {
        if (roomLocked || cameraConfiner == null || bossRoomBounds == null)
            return;

        originalBounds = cameraConfiner.BoundingShape2D;
        cameraConfiner.BoundingShape2D = bossRoomBounds;
        roomLocked = true;

        if (leftWall != null) leftWall.SetActive(true);
        if (rightWall != null) rightWall.SetActive(true);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (currentState != BossState.Rolling)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (rollDamageTimer > 0f)
            return;

        PlayerDeath playerDeath = collision.gameObject.GetComponent<PlayerDeath>();
        if (playerDeath != null)
        {
            playerDeath.TakeDamage(rollDamage);
            rollDamageTimer = rollDamageCooldown;

            KnockBack knockBack = collision.gameObject.GetComponent<KnockBack>();
            if (knockBack != null)
            {
                Vector2 direction = (Vector2)collision.transform.position - (Vector2)transform.position;
                direction = new Vector2(Mathf.Sign(direction.x), 0f);

                knockBack.ApplyKnockback(direction, rollKnockbackForce, rollKnockbackUpwardForce);
            }
        }
    }

    public new void TakeDamage(float damage)
    {
        if (currentState == BossState.Transitioning || currentHealth <= 0f)
            return;

        if (currentPhase == BossPhase.Phase2 && !isVulnerable)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        Debug.Log("Peach Boss took damage: " + damage + " | Health left: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        if (!phase2Triggered && currentHealth <= maxHealth * phase2Threshold)
        {
            phase2Triggered = true;
            TryInterruptForPhase2();
        }
    }

    void TryInterruptForPhase2()
    {
        if (currentState == BossState.Rolling)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            StartRollOutro();
        }
        else if (currentState == BossState.DustAttack)
        {
            StartPreTransition();
        }
    }

    void StartPreTransition()
    {
        currentState = BossState.PreTransition;
        stateTimer = preTransitionDelay;

        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("DustAttackEnd");
    }

    void UpdatePreTransition()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            StartTransitioning();
        }
    }

    void StartTransitioning()
    {
        currentState = BossState.Transitioning;
        stateTimer = transitionDuration;

        rb.linearVelocity = Vector2.zero;

        float directionX = player.position.x > transform.position.x ? 1f : -1f;

        if (seedLandingRenderer != null)
            seedLandingRenderer.flipX = directionX > 0f;

        if (normalCollider != null)
            normalCollider.enabled = false;

        if (seedCollider != null)
            seedCollider.enabled = true;

        if (animator != null)
            animator.SetTrigger("SeedTransition");

        Debug.Log("Transitioning");
    }

    void UpdateTransitioning()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            currentPhase = BossPhase.Phase2;
            StartPhase2();
        }
    }

    void StartPhase2()
    {
        currentState = BossState.Phase2Idle;
        stateTimer = phase2IdleDelay;

        if (shieldUI != null)
            shieldUI.SetShieldedInstant();
    }

    void UpdatePhase2Idle()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            StartJumpCycle();
        }
    }

    void StartJumpCycle()
    {
        currentJumpCount = 0;
        StartJumpRise();
    }

    void StartJumpRise()
    {
        currentState = BossState.Phase2JumpRise;
        jumpElapsed = 0f;
        jumpStartPos = transform.position;

        if (animator != null)
            animator.SetTrigger("Phase2JumpRise");
    }

    void UpdateJumpRise()
    {
        jumpElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(jumpElapsed / jumpRiseDuration);

        transform.position = jumpStartPos + Vector3.up * jumpHeight * t;

        if (t >= 1f)
        {
            StartJumpFall();
        }
    }

    void StartJumpFall()
    {
        currentState = BossState.Phase2JumpFall;
        jumpElapsed = 0f;
        jumpPeakPos = transform.position;

        if (animator != null)
            animator.SetTrigger("Phase2JumpFall");
    }

    void UpdateJumpFall()
    {
        jumpElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(jumpElapsed / jumpFallDuration);

        transform.position = Vector3.Lerp(jumpPeakPos, jumpStartPos, t);

        if (t >= 1f)
        {
            transform.position = jumpStartPos;
            currentJumpCount++;

            float directionX = player.position.x > transform.position.x ? 1f : -1f;
            FaceDirection(directionX);

            if (impulseSource != null)
                impulseSource.GenerateImpulse();

            Debug.Log(" Phase2: landed jump " + currentJumpCount + " / " + jumpsPerCycle);

            SpawnWave();

            StartJumpPause();
        }
    }

    void SpawnWave()
    {
        if (wavePrefab == null || player == null)
            return;

        float directionX = player.position.x > transform.position.x ? 1f : -1f;

        Vector3 spawnPos = transform.position + new Vector3(waveSpawnOffset.x * directionX, waveSpawnOffset.y, 0f);

        GameObject wave = Instantiate(wavePrefab, spawnPos, Quaternion.identity);

        WaveAttack waveScript = wave.GetComponent<WaveAttack>();
        if (waveScript != null)
            waveScript.Init(directionX, bossRoomBounds);
    }

    void StartJumpPause()
    {
        currentState = BossState.Phase2JumpPause;

        bool isLastJump = currentJumpCount >= jumpsPerCycle;
        stateTimer = isLastJump ? pauseBeforeCritical : jumpPauseDuration;
    }

    void UpdateJumpPause()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (currentJumpCount >= jumpsPerCycle)
            {
                StartCriticalState();
            }
            else
            {
                StartJumpRise();
            }
        }
    }

    void StartCriticalState()
    {
        currentState = BossState.Phase2Critical;
        stateTimer = criticalStateDuration;
        isVulnerable = true;

        if (shieldUI != null)
            shieldUI.BreakShield();

        if (animator != null)
            animator.SetTrigger("Phase2Critical");

        Debug.Log("Phase2: critical state");
    }

    void UpdateCriticalState()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            isVulnerable = false;

            if (shieldUI != null)
                shieldUI.RestoreShield();

            StartJumpCycle();
        }
    }

    void Die()
    {
        currentState = BossState.Dead;
        isVulnerable = false;

        rb.linearVelocity = Vector2.zero;

        OnBossDied?.Invoke();

        if (animator != null)
            animator.SetTrigger("Death");

        StartCoroutine(DeathSequence());
    }

    System.Collections.IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathAnimDelay);

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (seedLandingRenderer != null)
            seedLandingRenderer.enabled = false;

        if (seedCollider != null)
            seedCollider.enabled = false;

        if (explosionEffect != null)
            explosionEffect.Play();

        yield return new WaitForSeconds(2f);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.artifactAppearClip);

        if (artifactToReveal != null)
            artifactToReveal.SetActive(true);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}