using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TripleBDialogueTrigger : MonoBehaviour
{
    [Header("Intro Dialogue")]
    [SerializeField] Message[] introMessages;

    [Header("Tutorial Dialogue")]
    [SerializeField] Message[] tutorialMessages;

    [Header("After Laser Dialogue")]
    [SerializeField] private Message[] afterLaserMessages;

    [Header("Dialogue Actors")]
    [SerializeField] private Actor[] actors;

    [Header("References")]
    [SerializeField] DialougeManager dialougeManager;
    [SerializeField] TripleBFollowPlayer tripleBFollow;
    [SerializeField] TripleBLaser tripleBLaser;
    [SerializeField] RotTimer rotTimer;

    Collider2D col;

    private bool hasTriggered = false;
    private bool waitingForTripleB = false;
    private bool waitingForLaser = false;
    private bool finishingTutorial = false;

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

        StartIntroDialogue();
    }

    void StartIntroDialogue()
    {
       if(!FindReferences())
            return;

        bool dialogueStarted = dialougeManager.OpenDialouge(introMessages, actors);

        if(!dialogueStarted)
            return ;

        dialougeManager.OnDialougeFinished += OnIntroDialogueFinished ;
        
    }

    void OnIntroDialogueFinished()
    {
        dialougeManager.OnDialougeFinished -= OnIntroDialogueFinished ;

        if (tripleBFollow == null)
            return;

        waitingForTripleB = true;

        tripleBFollow.OnReachedPlayer += OnTripleBReachedPlayer;

        tripleBFollow.StartFollowing();
    }

    void OnTripleBReachedPlayer()
    {
        if(!waitingForTripleB)
            return ;

        waitingForTripleB = false;  

        tripleBFollow.OnReachedPlayer -= OnTripleBReachedPlayer;

        StartTutorialDialogue();
    }

    void StartTutorialDialogue()
    {
        if(!FindReferences()) return ;

        bool dialogueStarted = dialougeManager.OpenDialouge(tutorialMessages, actors);

        if (!dialogueStarted)
            return;

        dialougeManager.OnDialougeFinished += OnTutorialDialogueFinished;
    }

    void OnTutorialDialogueFinished()
    {
        dialougeManager.OnDialougeFinished -=OnTutorialDialogueFinished ;

        if(tripleBLaser == null)
            return ;

        waitingForLaser = true;

        tripleBLaser.OnLaserFinished += OnLaserFinished;
    }

    void OnLaserFinished()
    {
        if(!waitingForLaser)
            return ;

        waitingForLaser = false;

        tripleBLaser.OnLaserFinished -= OnLaserFinished;

        StartAfterLaserDialogue();
    }

    void StartAfterLaserDialogue()
    {
       if(!FindReferences() ) return ;

       finishingTutorial = true;

        bool dialogueStarted = dialougeManager.OpenDialouge(afterLaserMessages, actors);

        if (!dialogueStarted)
        {
            FinishTripleBTutorial();
            return;
        }

        dialougeManager.OnDialougeFinished += OnAfterLaserDialogueFinished;
    }

    void OnAfterLaserDialogueFinished()
    {
        dialougeManager.OnDialougeFinished -= OnAfterLaserDialogueFinished;

        FinishTripleBTutorial();

    }

    void FinishTripleBTutorial()
    {
        if(!finishingTutorial)
            return ;

        finishingTutorial = false;
        if(rotTimer != null)
        {
            rotTimer.RestoreFullTime();
            rotTimer.StartTimer();
        }
    }
    private bool FindReferences()
    {
        if (dialougeManager == null)
        {
            GameObject dialogueObject = GameObject.Find("DialougeManager");

            if (dialogueObject != null)
                dialougeManager = dialogueObject.GetComponent<DialougeManager>();
        }

        if (tripleBFollow == null)
        {
            Debug.LogError("Triple B Follow reference is missing.");
            return false;
        }

        if (dialougeManager == null)
        {
            Debug.LogError("Dialogue Manager not found.");
            return false;
        }

        return true;
    }

    private void OnDestroy()
    {
        if (dialougeManager != null)
        {
            dialougeManager.OnDialougeFinished -= OnIntroDialogueFinished;

            dialougeManager.OnDialougeFinished -= OnTutorialDialogueFinished;

            dialougeManager.OnDialougeFinished -= OnAfterLaserDialogueFinished;
        }

        if (tripleBFollow != null)
        {
            tripleBFollow.OnReachedPlayer -= OnTripleBReachedPlayer;
        }

        if (tripleBLaser != null)
        {
            tripleBLaser.OnLaserFinished -= OnLaserFinished;
        }

    }
}
