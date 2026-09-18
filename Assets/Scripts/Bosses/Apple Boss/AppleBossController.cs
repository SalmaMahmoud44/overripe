using UnityEngine;

public class AppleBossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;

    [SerializeField] Animator bodyAnimator;
    [SerializeField] Animator armAnimator;

    [SerializeField] AppleBossSeedAttack seedAttack;
    [SerializeField] AppleBossMovement bossMovement;
    [SerializeField] AppleBossStickAttack stickAttack;
    [SerializeField] AppleBossVulnerable vulnerable;

    public Transform Player => player;
    public Animator BodyAnimator => bodyAnimator;
    public Animator ArmAnimator => armAnimator;
    public AppleBossMovement BossMovement => bossMovement;

    private void Awake()
    {
        if (seedAttack == null)
            seedAttack = GetComponent<AppleBossSeedAttack>();

        if (bossMovement == null)
            bossMovement = GetComponent<AppleBossMovement>();  
        
        if (stickAttack == null)
            stickAttack = GetComponent<AppleBossStickAttack>();

        if (vulnerable == null)
            vulnerable = GetComponent<AppleBossVulnerable>();
    }

    public void StartSeedAttack()
    {
        if (seedAttack == null)
            return;

        seedAttack.StartAttack();
    }

    public void StartStickAttack()
    {
        if (stickAttack == null)
            return;

        stickAttack.StartAttack();
    }

    [ContextMenu("Test Seed Attack")]
    private void TestSeedAttack()
    {
        StartSeedAttack();
    }

    

    [ContextMenu("Move Left")]
    private void TestMoveLeft()
    {
        bossMovement.MoveToLeft();
    }

    [ContextMenu("Move Center")]
    private void TestMoveCenter()
    {
        bossMovement.MoveToCenter();
    }

    [ContextMenu("Move Right")]
    private void TestMoveRight()
    {
        bossMovement.MoveToRight();
    }

    [ContextMenu("Test Staff Attack")]
    private void TestStaffAttack()
    {
        StartStickAttack();
    }

    [ContextMenu("Test Vulnerable")]
    private void TestVulnerable()
    {
        if (vulnerable == null)
            return;

        vulnerable.StartTired();
    }
}
