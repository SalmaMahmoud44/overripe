using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TripleBDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] Message[] messages;
    [SerializeField] Actor[] actors;
    [SerializeField] DialougeManager dialougeManager;

    [Header("Triple B")]
    [SerializeField] TripleBFollowPlayer tripleBFollow;

    Collider2D col;
    private bool hasTriggered = false;

    private void Reset()
    {
        col = GetComponent<Collider2D>();

        if(col != null) 
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(hasTriggered) return;

        if(!collision.CompareTag("Player"))
            return;

        hasTriggered = true;

        StartDialogue();
    }

    void StartDialogue()
    {
        if(dialougeManager == null)
        {
            dialougeManager = GameObject.Find("DialougeManager").GetComponent<DialougeManager>();
        }

        if(tripleBFollow == null)
            return;

        if(dialougeManager == null)
            return ;

        bool dialogueStarted = dialougeManager.OpenDialouge(messages, actors);

        if(!dialogueStarted)
            return ;

        dialougeManager.OnDialougeFinished += OnDialougeFinished ;
        
    }

    void OnDialougeFinished()
    {
        dialougeManager.OnDialougeFinished -= OnDialougeFinished ;
        if(tripleBFollow != null)
        {
            tripleBFollow.StartFollowing();
        }
    }

    private void OnDestroy()
    {
        if(dialougeManager != null)
        {
            dialougeManager.OnDialougeFinished -= OnDialougeFinished;
        }
    }
}
