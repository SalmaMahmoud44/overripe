using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    [SerializeField] PlayerController controller;

    private void Awake()
    {
        if (controller == null) 
        controller = GetComponentInParent<PlayerController>();
    }
    public void OnJumpAnimationFinished()
    {
        if( controller != null)
            controller.OnJumpAnimationFinished();

    }
}
