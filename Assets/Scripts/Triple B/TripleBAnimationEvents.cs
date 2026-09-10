using UnityEngine;

public class TripleBAnimationEvents : MonoBehaviour
{
    [SerializeField] TripleBLaser tripleBLaser;

    public void AnimationFireLaser()
    {
        if (tripleBLaser != null)
        {
            tripleBLaser.AnimationFireLaser();
        }
    }

    public void HoldAttackAnimation()
    {
        if (tripleBLaser != null)
        {
            tripleBLaser.HoldAttackAnimation();
        }
    }

}
