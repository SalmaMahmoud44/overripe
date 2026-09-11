using System.Collections;
using UnityEngine;

public class JuiceSqueeze : MonoBehaviour
{
    [SerializeField] float rootDuration = 1f;

    public bool IsTriggered { get; private set; } = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsTriggered || !collision.CompareTag("Player"))
            return;

        PlayerController playerController = collision.GetComponent<PlayerController>();
        if (playerController != null)
        {
            IsTriggered = true;
            StartCoroutine(RootPlayerThenDisappear(playerController));
        }
    }

    IEnumerator RootPlayerThenDisappear(PlayerController playerController)
    {
        PlayerDeath playerDeath = playerController.GetComponent<PlayerDeath>();
        if (playerDeath != null)
            playerDeath.TakeDamage(2f);

        KnockBack knockBack = playerController.GetComponent<KnockBack>();

        if (knockBack != null)
        {
            knockBack.PlayHitAnimation();
        }


        playerController.SetControlsLocked(true);
        yield return new WaitForSeconds(rootDuration);
        playerController.SetControlsLocked(false);
        Destroy(gameObject);
    }
}