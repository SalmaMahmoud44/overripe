using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;


public class PeachBossController : MonoBehaviour
{
    public enum BossState { Idle, RollIntro, Rolling, RollOutro, DustAttack }


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
    float rollDirectionX;
    float stateTimer;
    int currentBounceCount;
    bool doingFinalHalfMove = false;
    float halfMoveTargetX;

    [Header("Dust Attack Settings")]
    [SerializeField] GameObject dustPuffPrefab;
    [SerializeField] int dustPuffCount = 4;
    [SerializeField] float dustPuffInterval = 0.5f;
    [SerializeField] Vector2 dustSpawnOffset = Vector2.zero;
    [SerializeField] float dustSpacing = 2f;
    [SerializeField] float dustAttackEndDelay = 1.5f;


    int currentDustPuffCount;
    bool waitingForDustEnd = false;

    bool playerSpotted = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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
    }

    void UpdateRollOutro()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            StartDustAttack();
        }
    }

    void StartDustAttack()
    {
        currentState = BossState.DustAttack;
        currentDustPuffCount = 0;
        waitingForDustEnd = false;
        stateTimer = dustPuffInterval;

        SpawnDustPuff();
    }

    void UpdateDustAttack()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (waitingForDustEnd)
            {
                StartRollIntro();
                return;
            }

            currentDustPuffCount++;

            if (currentDustPuffCount >= dustPuffCount)
            {
                waitingForDustEnd = true;
                stateTimer = dustAttackEndDelay;
                return;
            }

            SpawnDustPuff();
            stateTimer = dustPuffInterval;
        }
    }

    void SpawnDustPuff()
    {
        if (dustPuffPrefab == null || player == null)
            return;

        float directionX = player.position.x > transform.position.x ? 1f : -1f;
        Vector2 direction = new Vector2(directionX, 0f);

        float spawnOffsetX = dustSpawnOffset.x + (directionX * dustSpacing * currentDustPuffCount);
        Vector3 spawnPos = transform.position + new Vector3(spawnOffsetX, dustSpawnOffset.y, 0f);

        GameObject puff = Instantiate(dustPuffPrefab, spawnPos, Quaternion.identity);

        DustPuff puffScript = puff.GetComponent<DustPuff>();
        if (puffScript != null)
            puffScript.Init(direction, bossRoomBounds);
    }

    void MoveAndBounce()
    {
        rb.linearVelocity = new Vector2(rollDirectionX * rollSpeed, rb.linearVelocity.y);

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

    void RegisterBounce()
    {
        currentBounceCount++;

        if (currentBounceCount >= rollBounceCount)
        {
            StartRollOutro();
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}