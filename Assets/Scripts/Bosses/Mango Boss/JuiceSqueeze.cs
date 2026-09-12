using System.Collections;
using UnityEngine;

public class JuiceSqueeze : MonoBehaviour
{
    [Header("Juice Settings")]
    [SerializeField] float rootDuration = 1f;

    [Header("Player Animation")]
    [SerializeField] string juiceAnimationBool = "inJuice";

    public bool IsTriggered { get; private set; } = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsTriggered || !collision.CompareTag("Player"))
            return;

        PlayerDeath playerDeath = collision.GetComponent<PlayerDeath>();

        if (playerDeath != null && playerDeath.IsDead)
            return;

        PlayerController playerController = collision.GetComponent<PlayerController>();
        if (playerController != null)
        {
            IsTriggered = true;

            if (AudioManager.Instance != null)AudioManager.Instance.PlaySFX(AudioManager.Instance.juiceHit);

            StartCoroutine(RootPlayerThenDisappear(playerController));
        }
    }

    IEnumerator RootPlayerThenDisappear(PlayerController playerController)
    {
        PlayerDeath playerDeath = playerController.GetComponent<PlayerDeath>();

   
        if (playerDeath != null)
            playerDeath.TakeDamage(2f);

        if (playerDeath != null && playerDeath.IsDead)
            yield break;


        playerController.SetControlsLocked(true);

        Animator playerAnimator = playerController.GetComponentInChildren<Animator>();


        if (playerAnimator != null)
            playerAnimator.SetBool(juiceAnimationBool, true);

            yield return new WaitForSeconds(rootDuration);


        if (playerAnimator != null)
            playerAnimator.SetBool(juiceAnimationBool, false);

        playerController.SetControlsLocked(false);
            Destroy(gameObject);
    }
}