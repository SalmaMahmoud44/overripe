using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialougeTrigger : MonoBehaviour
{
    [Header("Dialouge Settings")]
    [SerializeField] private Message[] messages;
    [SerializeField] private Actor[] actors;
    [SerializeField] private DialougeManager dialougeManager;
    [SerializeField] private string playerTag = "Player";

    [Header("Progress / Checkpoint")]
    [SerializeField] private string dialogueID;
    [SerializeField] private bool saveCheckpointAfterDialogue = false;
    [SerializeField] private Checkpoint checkpointAfterDialogue;

    [Header("Timer Settings")]
    [SerializeField] private bool startTimerAfterDialogue = false;
    [SerializeField] private RotTimer rotTimer;

    private bool hasTriggered = false;


    private void Reset()
    {
        Collider2D collider = GetComponent<Collider2D>();

        if (collider != null)
            collider.isTrigger = true;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered)
            return;

        if (!collision.CompareTag(playerTag))
            return;

        hasTriggered = true;

        StartDialouge();
    }


    public void StartDialouge()
    {

        if (CheckpointManager.Instance != null && !string.IsNullOrEmpty(dialogueID) && CheckpointManager.Instance.IsDialogueCompleted(dialogueID))
        {

            hasTriggered = true;

            if (startTimerAfterDialogue)
            {
                FindRotTimer();

                if (rotTimer != null)
                    rotTimer.StartTimer();
            }

            return;
        }



        if (dialougeManager == null)
        {
            GameObject managerObject = GameObject.Find("DialougeManager");

            if (managerObject != null)
                dialougeManager = managerObject.GetComponent<DialougeManager>();
        }

        if (rotTimer == null)
            FindRotTimer();
 


        if (dialougeManager == null)
            return;
 

        if (dialougeManager.OpenDialouge(messages, actors))
            dialougeManager.OnDialougeFinished += HandleDialogueFinished;
 
    }


    private void HandleDialogueFinished()
    {
        if (dialougeManager != null)
            dialougeManager.OnDialougeFinished -= HandleDialogueFinished;




        if (CheckpointManager.Instance != null && !string.IsNullOrEmpty(dialogueID))
        {
            CheckpointManager.Instance.MarkDialogueCompleted( dialogueID);
        }


        if (saveCheckpointAfterDialogue && checkpointAfterDialogue != null)
        {
            checkpointAfterDialogue.Activate();
        }

        if (startTimerAfterDialogue)
        {
            FindRotTimer();

            if (rotTimer != null)
                rotTimer.StartTimer();
        }
    }


    private void FindRotTimer()
    {
        GameObject timerObject =
            GameObject.Find("RotTimerCanvas");

        if (timerObject != null)
            rotTimer = timerObject.GetComponent<RotTimer>();
    }


    private void OnDestroy()
    {
        if (dialougeManager != null)
            dialougeManager.OnDialougeFinished -= HandleDialogueFinished;
    }
}



public enum PlayerAction
{
    None,
    Continue,
    Move,
    Jump,
    Dash,
    Melee,
    Shoot,
    Laser,
}

[System.Serializable]
public class Message
{
    public int actorId;
    public string message;
    public PlayerAction waitForAction = PlayerAction.None;
    public bool LockPlayerControls = true;
}

[System.Serializable]
public class Actor
{
    public string name;
    public Sprite sprite;
}