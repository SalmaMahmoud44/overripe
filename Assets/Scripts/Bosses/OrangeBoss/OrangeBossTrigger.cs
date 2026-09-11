using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class OrangeBossTrigger : MonoBehaviour
{

    [SerializeField] OrangeBossController bossController;
    [SerializeField] BossHealthUI bossHealthUI;

    [SerializeField] float delayBeforeeHealthBar= 0f;
    [SerializeField] float delayBeforeStartFight = 1f;

    [SerializeField] string playerTag = "Player";
    [SerializeField] bool lockPlayerControlsDuringIntro = false;
    [SerializeField] PlayerController playerController;


    bool triggered = false;


    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) 
            return;
        if(!collision.CompareTag(playerTag)) return;

        triggered = true;
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(StartIntro());
    }

    IEnumerator StartIntro()
    {
        if (lockPlayerControlsDuringIntro && playerController != null)
            playerController.SetControlsLocked(true);

        yield return new WaitForSeconds(delayBeforeeHealthBar);

        if (bossHealthUI != null)
            bossHealthUI.Show();

        yield return new WaitForSeconds(delayBeforeStartFight);

        if (lockPlayerControlsDuringIntro && playerController != null)
            playerController.SetControlsLocked(false);

        if (bossController != null)
            bossController.BeginFight();
    }
}
