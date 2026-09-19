using UnityEngine;
using UnityEngine.Rendering;

public class AppleBossAnimationEvents : MonoBehaviour
{
    [SerializeField] AppleBossController appleBoss;
    [SerializeField] AppleBossStickAttack stickAttack;
    [SerializeField] AppleBossVulnerable bossVulnerable;

    private void Awake()
    {
        if (appleBoss == null)
            appleBoss = GetComponentInParent<AppleBossController>();

        if (stickAttack == null)
            stickAttack = GetComponentInParent<AppleBossStickAttack>();

        if (bossVulnerable == null)
            bossVulnerable = GetComponentInParent<AppleBossVulnerable>();

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

    public void OnStickHitboxOn()
    {
        if (stickAttack != null)
            stickAttack.OnStickHitboxOn();
    }

    public void OnStickHitboxOff()
    {
        if (stickAttack != null)
            stickAttack.OnStickHitboxOff();
    }

    public void FinishAttack()
    {
        if (stickAttack != null)
            stickAttack.FinishAttack();
    }

    public void BecomeVulnerable()
    {
        if (bossVulnerable != null)
            bossVulnerable.BecomeVulnerable();
    }
    public void StickImpact()
    {
        if(stickAttack != null)
            stickAttack.OnStickImpact();
    }
}
