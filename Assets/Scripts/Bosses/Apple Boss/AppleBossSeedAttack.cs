using UnityEngine;

public class AppleBossSeedAttack : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] AppleBossController appleBoss;

    [Header("Seed")]
    [SerializeField] GameObject seedPrefab;

    [Header("Seed Spawn Pints")]
    [SerializeField] private Transform frontSpawn;
    [SerializeField] private Transform rightCloseSpawn;
    [SerializeField] private Transform rightFarSpawn;
    [SerializeField] private Transform leftCloseSpawn;
    [SerializeField] private Transform leftFarSpawn;

    [Header("Range")]
    [SerializeField] float closeRange = 5f;
    [SerializeField] float frontRange = 0.5f;


    bool attackActive;

    Transform currentSpawnPoint;

    private void Awake()
    {
        if(appleBoss == null) 
            appleBoss = GetComponent<AppleBossController>();
    }

    public void StartAttack()
    {
        if (attackActive)
            return;

        if(appleBoss.Player == null)
        {
            Debug.Log("There is no Player ref");
            return;
        }

        attackActive = true;

        string animationTrigger = GetSeedAnimationTrigger();

        currentSpawnPoint = GetSeedSpawnPoint(animationTrigger);

        appleBoss.BodyAnimator.SetTrigger(animationTrigger);

        appleBoss.ArmAnimator.SetTrigger("SeedAttack");
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
        float distanceX = appleBoss.Player.position.x - transform.position.x;
        float distance = Mathf.Abs(distanceX);

        bool playerIsRight = distanceX > 0f;

        if (distance <= frontRange)
            return "SeedFront";

        bool isClose = distance<=closeRange;

        if (playerIsRight)
            return isClose ? "SeedRightClose" : "SeedRightFar";
        else
            return isClose ? "SeedLeftClose" : "SeedLeftFar";

    }

    public void OnSeedSpawn()
    {
        if (!attackActive) 
            return;

        if(seedPrefab == null)
            return;

        if(currentSpawnPoint == null)
            return;

        GameObject appleSeed = Instantiate(seedPrefab,currentSpawnPoint.position,Quaternion.identity);

        AppleSeedProjectile projectile = appleSeed.GetComponent<AppleSeedProjectile>(); 

        if(projectile != null)
            projectile.Initialize(appleBoss.Player.position);


    }
    public void OnAttackFinished()
    {
        attackActive = false;
        currentSpawnPoint = null;
    }

   
}
