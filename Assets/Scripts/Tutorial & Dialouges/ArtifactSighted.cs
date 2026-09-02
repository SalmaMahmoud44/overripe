using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ArtifactSighted : MonoBehaviour
{
    [Header("Dialouge Settings")]
    [SerializeField] private DialougeManager dialougeManager;
    [SerializeField] private Message[] messages;
    [SerializeField] private Actor[] actors;
    [SerializeField] string playerTag = "Player";

    bool isTriggered = false;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered || !collision.CompareTag(playerTag)) return;
        isTriggered = true;

        if (dialougeManager == null)
            dialougeManager = GameObject.Find("DialougeManager").GetComponent<DialougeManager>();

        dialougeManager.OpenDialouge(messages, actors);
    }
}
