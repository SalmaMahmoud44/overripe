using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PeachBossTrigger : MonoBehaviour
{
    [SerializeField] PeachBossController bossController;
    [SerializeField] string playerTag = "Player";

    bool triggered = false;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered || !collision.CompareTag(playerTag)) return;

        triggered = true;
        GetComponent<Collider2D>().enabled = false;

        if (bossController != null)
            bossController.BeginFight();
    }
}