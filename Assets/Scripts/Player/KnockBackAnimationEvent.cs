using UnityEngine;

public class KnockBackAnimationEvent : MonoBehaviour
{
    [SerializeField] KnockBack knockBack;

    public void FallTriggerEvent()
    {
        if (knockBack != null)
            knockBack.OnKnockbackFall();
    }
}
