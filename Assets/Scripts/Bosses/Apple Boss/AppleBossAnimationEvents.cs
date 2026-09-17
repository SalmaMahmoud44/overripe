using UnityEngine;

public class AppleBossAnimationEvents : MonoBehaviour
{
    [SerializeField] AppleBossController appleBoss;

    private void Awake()
    {
        if (appleBoss == null)
            appleBoss = GetComponentInParent<AppleBossController>();

    }

    public void SeedSpawn()
    {
        AppleBossSeedAttack attack = appleBoss.GetComponent<AppleBossSeedAttack>();

        if (attack != null)
            attack.OnSeedSpawn();
    }

    public void SeedAttackFinished()
    {
        AppleBossSeedAttack attack = appleBoss.GetComponent<AppleBossSeedAttack>();

        if (attack != null)
            attack.OnAttackFinished();
    }
}
