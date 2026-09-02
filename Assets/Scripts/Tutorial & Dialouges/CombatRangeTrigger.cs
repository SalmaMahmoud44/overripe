using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CombatRangeTrigger : MonoBehaviour
{
    [SerializeField] RotTimer rotTimer;
    [SerializeField] string playerTag = "Player";

    bool isTriggered = false;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(isTriggered || !collision.CompareTag(playerTag))  return;
        isTriggered = true;
        if (rotTimer == null)
                 rotTimer= GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();

            rotTimer.StartTimer();
    }
}
