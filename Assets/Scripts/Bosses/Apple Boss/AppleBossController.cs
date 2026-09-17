using UnityEngine;

public class AppleBossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;

    [SerializeField] Animator bodyAnimator;
    [SerializeField] Animator armAnimator;

    [SerializeField] AppleBossSeedAttack seedAttack;

    public Transform Player => player;
    public Animator BodyAnimator => bodyAnimator;
    public Animator ArmAnimator => armAnimator;

    private void Awake()
    {
        if (seedAttack == null)
            seedAttack = GetComponent<AppleBossSeedAttack>();
    }

    public void StartSeedAttack()
    {
        if (seedAttack == null)
            return;

        seedAttack.StartAttack();
    }

    [ContextMenu("Test Seed Attack")]
    private void TestSeedAttack()
    {
        StartSeedAttack();
    }
}
