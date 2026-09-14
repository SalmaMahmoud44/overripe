using UnityEngine;

public class OrangeSoliderAttackEvent : MonoBehaviour
{
    [SerializeField] OrangeSolider orangeSolider;

    public void DoAttackAnimationEvent()
    {
        if (orangeSolider != null) 
            orangeSolider.DealAttackDamage();
    }

    public void StopAttackAnimationEvent()
    {
        if (orangeSolider != null)
            orangeSolider.FinishAttack();
    }

}
