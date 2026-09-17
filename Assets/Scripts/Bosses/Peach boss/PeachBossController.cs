using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PeachBossController : MonoBehaviour, IDamagable
{
    public enum BossState { Idle, RollIntro, Rolling, RollOutro, DustAttack, PreTransition, Transitioning, Phase2 }
    public enum BossPhase { Phase1, Phase2 }

    [Header("Health Settings")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth;

    [Header("UI")]
    [SerializeField] Slider healthSlider;

    [Header("Room Walls")]
    [SerializeField] GameObject leftWall;
    [SerializeField] GameObject rightWall;

    [Header("Detection Settings")]
    [SerializeField] float detectRange = 6f;
    Transform player;

    [Header("Colliders")]
    [SerializeField] Collider2D normalCollider;
    [SerializeField] Collider2D rollCollider;

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

            case BossState.Phase2:
                // هنضيف UpdatePhase2() هنا لما نبدأ نبني اللوجيك الفعلي
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
        rollDirectionX = player.position.x > transform.position.x ? 1f : -1f;
        currentBounceCount = 0;
        doingFinalHalfMove = false;

        SetRollCollider(true);
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

        float directionX = player.position.x > transform.position.x ? 1f : -1f;
        Vector2 direction = new Vector2(directionX, 0f);

        Vector3 spawnPos = transform.position + new Vector3(dustSpawnOffset.x, dustSpawnOffset.y, 0f);

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

    public new void TakeDamage(float damage)
    {
        if (currentState == BossState.Transitioning || currentState == BossState.Phase2 || currentHealth <= 0f)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        Debug.Log("Peach Boss took damage: " + damage + " | Health left: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Debug.Log("Peach Boss died (placeholder)");
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

        if (animator != null)
            animator.SetTrigger("SeedTransition");

        Debug.Log("Peach Boss: Transitioning to Phase 2 (placeholder)");
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
        currentState = BossState.Phase2;

        Debug.Log("Peach Boss: Phase 2 logic starts here (placeholder)");
        // هنبني هنا لوجيك النط والـ wave في الخطوة الجاية
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}