using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
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
    [SerializeField] CinemachineImpulseSource impulseSource;
    [SerializeField] CinemachineConfiner2D cameraConfiner;
    [SerializeField] Collider2D bossRoomBounds;
    [SerializeField] ParticleSystem explosionEffect;
    [SerializeField] float deathAnimDelay = 1f;
    [SerializeField] GameObject artifactToReveal;
    Collider2D originalBounds;
    bool roomLocked = false;

    [Header("Jump Shadow Settings")]
    [SerializeField] SpriteRenderer jumpShadow;
    [SerializeField] Vector3 shadowMaxScale = new Vector3(1.5f, 1.5f, 1f);
    [SerializeField] Color shadowFadedColor = new Color(0f, 0f, 0f, 0.2f);
    [SerializeField] Color shadowClearColor = new Color(0f, 0f, 0f, 0.8f);
    [SerializeField] float shadowGrowDuration = 1f;
    [SerializeField] float shadowYOffset = 0f;

    [Header("Jump Slam Settings")]
    [SerializeField] float jumpHeight = 8f;
    [SerializeField] float riseDuration = 0.4f;
    [SerializeField] float fallDuration = 0.3f;
    [SerializeField] float pauseAfterLanding = 0.5f;
    [SerializeField] float delayBeforeAttack = 1.5f;
    [SerializeField] float landDamage = 5f;
    [SerializeField] float enragedSpeedMultiplier = 0.6f;

    [Header("Jump Slam Knockback")]
    [SerializeField] float knockbackForce = 20f;
    [SerializeField] float knockbackUpwardForce = 22f;

    [Header("Attack Cycle Settings")]
    [SerializeField] int normalJumpCount = 2;
    [SerializeField] int enragedJumpCount = 4;

    [Header("Juice Squeeze Settings")]
    [SerializeField] GameObject puddlePrefab;
    [SerializeField] int normalPuddleCount = 5;
    [SerializeField] int enragedPuddleCount = 8;
    [SerializeField] float normalOozeDuration = 2f;
    [SerializeField] float enragedOozeDuration = 4f;
    [SerializeField] float spreadRadius = 4f;
    [SerializeField] float puddleYOffset = 0f;
    [SerializeField] ParticleSystem juiceFountain;
    [SerializeField] ParticleSystem juiceRain;
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

        if (currentState == BossState.Idle)
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

        LockCameraToBossRoom();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossMusic();


        attackRoutine = StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(delayBeforeAttack);

        while (currentState == BossState.Attacking)
        {
            int jumpsThisCycle = isEnraged ? enragedJumpCount : normalJumpCount;

            for (int i = 0; i < jumpsThisCycle; i++)
            {
                yield return StartCoroutine(JumpSlamAttack());
            }

            yield return StartCoroutine(JuiceSqueezeAttack());
        }
    }

    IEnumerator JumpSlamAttack()
    {
        float speedMultiplier = isEnraged ? enragedSpeedMultiplier : 1f;
        float currentRiseDuration = riseDuration * speedMultiplier;
        float currentFallDuration = fallDuration * speedMultiplier;
        float currentPauseAfterLanding = pauseAfterLanding * speedMultiplier;

        Vector3 startPos = transform.position;
        Vector3 bossLandingPos = new Vector3(player.position.x, startPos.y, startPos.z);
        Vector3 shadowPos = new Vector3(player.position.x, player.position.y + shadowYOffset, startPos.z);
        Vector3 risePos = startPos + Vector3.up * jumpHeight;
        Vector3 fallStartPos = new Vector3(bossLandingPos.x, risePos.y, bossLandingPos.z);

        if (animator != null)
            animator.SetTrigger("Jump");

        StartCoroutine(GrowShadow(shadowPos, shadowGrowDuration));

        float elapsed = 0f;
        while (elapsed < currentRiseDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, risePos, elapsed / currentRiseDuration);
            yield return null;
        }

        float remainingHangTime = shadowGrowDuration - currentRiseDuration - currentFallDuration;
        if (remainingHangTime > 0f)
            yield return new WaitForSeconds(remainingHangTime);

        Flip();

        if (animator != null)
            animator.SetTrigger("Smash");

        elapsed = 0f;
        while (elapsed < currentFallDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(fallStartPos, bossLandingPos, elapsed / currentFallDuration);
            yield return null;
        }

        transform.position = bossLandingPos;
        CheckLandingHit();

        if (jumpShadow != null)
            jumpShadow.gameObject.SetActive(false);

        yield return new WaitForSeconds(currentPauseAfterLanding);
    }

    void CheckLandingHit()
    {

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.mangoSmash);

        if (impulseSource != null)
            impulseSource.GenerateImpulse();

        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D playerCollider = player.GetComponent<Collider2D>();

        if (myCollider != null && playerCollider != null && myCollider.bounds.Intersects(playerCollider.bounds))
        {
            PlayerDeath playerDeath = player.GetComponent<PlayerDeath>();
            if (playerDeath != null)
            {
                playerDeath.TakeDamage(landDamage);

                if (playerDeath.IsDead)
                    return;
            }


            KnockBack knockBack = player.GetComponent<KnockBack>();
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (knockBack != null && playerController != null)
            {
                float directionX = player.position.x > transform.position.x? 1f: -1f;

                Vector2 direction = new Vector2(directionX, 0f);

                knockBack.ApplyKnockback(direction,knockbackForce,knockbackUpwardForce);
            }
        }
    }

    IEnumerator JuiceSqueezeAttack()
    {
        if (animator != null)
            animator.SetBool("IsSqueezing", true);

        if (juiceFountain != null)
            juiceFountain.Play();

        if (juiceRain != null)
            juiceRain.Play();

        int puddleCount = isEnraged ? enragedPuddleCount : normalPuddleCount;
        float oozeDuration = isEnraged ? enragedOozeDuration : normalOozeDuration;

        float intervalBetweenPuddles = oozeDuration / puddleCount;
        List<GameObject> spawnedPuddles = new List<GameObject>();

        for (int i = 0; i < puddleCount; i++)
        {
            GameObject puddle = SpawnPuddle();
            if (puddle != null)
                spawnedPuddles.Add(puddle);

            yield return new WaitForSeconds(intervalBetweenPuddles);
        }

        if (juiceFountain != null)
            juiceFountain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (juiceRain != null)
            juiceRain.Stop();

        if (animator != null)
            animator.SetBool("IsSqueezing", false);

        foreach (GameObject puddle in spawnedPuddles)
        {
            if (puddle == null)
                continue;

            JuiceSqueeze puddleScript = puddle.GetComponent<JuiceSqueeze>();
            if (puddleScript != null && !puddleScript.IsTriggered)
            {
                Destroy(puddle);
            }
        }
    }

    GameObject SpawnPuddle()
    {
        if (puddlePrefab == null)
            return null;

        float randomX = Random.Range(-spreadRadius, spreadRadius);
        Vector2 rayOrigin = new Vector2(transform.position.x + randomX, transform.position.y + 5f);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 15f, LayerMask.GetMask("Ground"));

        if (hit.collider == null)
            return null;

        Vector3 spawnPos = new Vector3(hit.point.x, hit.point.y + puddleYOffset, transform.position.z);

        return Instantiate(puddlePrefab, spawnPos, Quaternion.identity);
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

        if (juiceFountain != null)
            juiceFountain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (juiceRain != null)
            juiceRain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (animator != null)
            animator.SetTrigger("Die");

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathAnimDelay);

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        if (explosionEffect != null)
            explosionEffect.Play();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.mangoDeath);

        Debug.Log("Mango Boss died");

        yield return new WaitForSeconds(2f);

        if (artifactToReveal != null)
            artifactToReveal.SetActive(true);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }

    void LockCameraToBossRoom()
    {
        if (roomLocked || cameraConfiner == null || bossRoomBounds == null)
            return;

        originalBounds = cameraConfiner.BoundingShape2D;
        cameraConfiner.BoundingShape2D = bossRoomBounds;
        roomLocked = true;
    }
}