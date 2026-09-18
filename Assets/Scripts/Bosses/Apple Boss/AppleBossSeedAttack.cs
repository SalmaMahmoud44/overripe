using System.Collections;
using UnityEngine;

public class AppleBossSeedAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] AppleBossController appleBoss;


    [Header("Seed")]
    [SerializeField] GameObject seedPrefab;


    [Header("Seed Spawn Points")]
    [SerializeField] Transform frontSpawn;
    [SerializeField] Transform rightCloseSpawn;
    [SerializeField] Transform rightFarSpawn;
    [SerializeField] Transform leftCloseSpawn;
    [SerializeField] Transform leftFarSpawn;


    [Header("Range")]
    [SerializeField] float closeRange = 5f;
    [SerializeField] float frontRange = 0.5f;


    [Header("Phase Seed Count")]
    [SerializeField] int phase1SeedCount = 1;
    [SerializeField] int phase2SeedCount = 2;
    [SerializeField] int phase3SeedCount = 3;


    [Header("Phase Seed Speed")]
    [SerializeField] float phase1SeedSpeed = 10f;
    [SerializeField] float phase2SeedSpeed = 12f;
    [SerializeField] float phase3SeedSpeed = 14f;


    [Header("Burst")]
    [SerializeField] float delayBetweenSeeds = 0.15f;


    Transform currentSpawnPoint;

    bool attackActive;

    Coroutine burstRoutine;


    public bool IsAttacking => attackActive;


    private void Awake()
    {
        if (appleBoss == null)
            appleBoss = GetComponent<AppleBossController>();
    }


    public void StartAttack()
    {
        if (attackActive)
            return;


        if (appleBoss.Player == null)
        {
            Debug.Log("There is no Player ref");
            return;
        }


        attackActive = true;


        string animationTrigger =
            GetSeedAnimationTrigger();


        currentSpawnPoint =
            GetSeedSpawnPoint(animationTrigger);


        if (appleBoss.BodyAnimator != null)
        {
            appleBoss.BodyAnimator.ResetTrigger(animationTrigger);
            appleBoss.BodyAnimator.SetTrigger(animationTrigger);
        }


        if (appleBoss.ArmAnimator != null)
        {
            appleBoss.ArmAnimator.ResetTrigger("SeedAttack");
            appleBoss.ArmAnimator.SetTrigger("SeedAttack");
        }
    }


    Transform GetSeedSpawnPoint(string trigger)
    {
        switch (trigger)
        {
            case "SeedFront":
                return frontSpawn;

            case "SeedRightClose":
                return rightCloseSpawn;

            case "SeedRightFar":
                return rightFarSpawn;

            case "SeedLeftClose":
                return leftCloseSpawn;

            case "SeedLeftFar":
                return leftFarSpawn;
        }


        return frontSpawn;
    }


    string GetSeedAnimationTrigger()
    {
        float distanceX =
            appleBoss.Player.position.x -
            transform.position.x;


        float distance =
            Mathf.Abs(distanceX);


        bool playerIsRight =
            distanceX > 0f;


        if (distance <= frontRange)
            return "SeedFront";


        bool isClose =
            distance <= closeRange;


        if (playerIsRight)
        {
            return isClose
                ? "SeedRightClose"
                : "SeedRightFar";
        }


        return isClose
            ? "SeedLeftClose"
            : "SeedLeftFar";
    }


    public void OnSeedSpawn()
    {
        if (!attackActive)
            return;


        if (seedPrefab == null)
            return;


        if (currentSpawnPoint == null)
            return;


        if (burstRoutine != null)
            StopCoroutine(burstRoutine);


        burstRoutine =
            StartCoroutine(SeedBurstRoutine());
    }


    private IEnumerator SeedBurstRoutine()
    {
        int seedCount =
            GetSeedCountForPhase();


        for (int i = 0; i < seedCount; i++)
        {
            SpawnSeed();


            if (i < seedCount - 1)
            {
                yield return new WaitForSeconds(
                    delayBetweenSeeds
                );
            }
        }


        burstRoutine = null;
    }


    void SpawnSeed()
    {
        if (currentSpawnPoint == null ||
            seedPrefab == null ||
            appleBoss.Player == null)
        {
            return;
        }


        GameObject appleSeed =
            Instantiate(
                seedPrefab,
                currentSpawnPoint.position,
                Quaternion.identity
            );


        AppleSeedProjectile projectile =
            appleSeed.GetComponent<AppleSeedProjectile>();


        if (projectile != null)
        {
            projectile.Initialize(
                appleBoss.Player.position,
                GetSeedSpeedForPhase()
            );
        }
    }


    int GetSeedCountForPhase()
    {
        switch (appleBoss.CurrentPhase)
        {
            case AppleBossController.BossPhase.Phase1:
                return phase1SeedCount;

            case AppleBossController.BossPhase.Phase2:
                return phase2SeedCount;

            case AppleBossController.BossPhase.Phase3:
                return phase3SeedCount;
        }


        return phase1SeedCount;
    }


    float GetSeedSpeedForPhase()
    {
        switch (appleBoss.CurrentPhase)
        {
            case AppleBossController.BossPhase.Phase1:
                return phase1SeedSpeed;

            case AppleBossController.BossPhase.Phase2:
                return phase2SeedSpeed;

            case AppleBossController.BossPhase.Phase3:
                return phase3SeedSpeed;
        }


        return phase1SeedSpeed;
    }


    public void OnAttackFinished()
    {
        attackActive = false;

        currentSpawnPoint = null;


        if (burstRoutine != null)
        {
            StopCoroutine(burstRoutine);
            burstRoutine = null;
        }
    }
}

