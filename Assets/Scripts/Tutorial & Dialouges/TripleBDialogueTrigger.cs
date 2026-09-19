using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TripleBDialogueTrigger : MonoBehaviour
{
    [Header("Intro Dialogue")]
    [SerializeField] private Message[] introMessages;

    [Header("Tutorial Dialogue")]
    [SerializeField] private Message[] tutorialMessages;

    [Header("After Laser Dialogue")]
    [SerializeField] private Message[] afterLaserMessages;

    [Header("Dialogue Actors")]
    [SerializeField] private Actor[] actors;

    [Header("References")]
    [SerializeField] private DialougeManager dialougeManager;
    [SerializeField] private TripleBFollowPlayer tripleBFollow;
    [SerializeField] private TripleBLaser tripleBLaser;
    [SerializeField] private RotTimer rotTimer;

    [Header("Checkpoints")]
    [SerializeField] private Checkpoint introCheckpoint;
    [SerializeField] private Checkpoint laserTutorialCheckpoint;
    [SerializeField] private Checkpoint laserUsedCheckpoint;

    [Header("Dialogue IDs")]
    [SerializeField] private string introDialogueID = "TripleB_Intro";
    [SerializeField] private string tutorialDialogueID = "TripleB_LaserTutorial";
    [SerializeField] private string afterLaserDialogueID = "TripleB_AfterLaser";

    private bool hasTriggered = false;
    private bool waitingForTripleB = false;
    private bool waitingForLaser = false;
    private bool finishingTutorial = false;

    private Collider2D col;


    private void Awake()
    {
        col = GetComponent<Collider2D>();

        if (col != null)
            col.isTrigger = true;
    }


    private void Start()
    {
        ResumeFromSavedProgress();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered)
            return;

        if (!collision.CompareTag("Player"))
            return;

        hasTriggered = true;

        StartIntroDialogue();
    }



    private void ResumeFromSavedProgress()
    {
        if (CheckpointManager.Instance == null)
            return;

        CheckpointManager.TripleBStage stage = CheckpointManager.Instance.CurrentTripleBStage;


        switch (stage)
        {
            case CheckpointManager.TripleBStage.None:
                break;
            case CheckpointManager.TripleBStage.IntroFinished:

                hasTriggered = true;
                StartTripleBFollowing(true);

                break;
            case CheckpointManager.TripleBStage.LaserTutorialFinished:

                hasTriggered = true;

                StartTripleBFollowing();
                StartWaitingForLaser();

                break;
            case CheckpointManager.TripleBStage.LaserUsed:

                hasTriggered = true;

                StartTripleBFollowing();
                StartAfterLaserDialogue();

                break;
            case CheckpointManager.TripleBStage.Completed:

                hasTriggered = true;
                StartTripleBFollowing();

                break;
        }
    }



    private void StartIntroDialogue()
    {
        if (!FindReferences())
            return;

        bool dialogueStarted = dialougeManager.OpenDialouge(introMessages, actors);

        if (!dialogueStarted)
            return;

        dialougeManager.OnDialougeFinished += OnIntroDialogueFinished;
    }


    private void OnIntroDialogueFinished()
    {
        dialougeManager.OnDialougeFinished -= OnIntroDialogueFinished;



        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.MarkDialogueCompleted(introDialogueID);

            CheckpointManager.Instance.SetTripleBStage(CheckpointManager.TripleBStage.IntroFinished);
        }

        if (introCheckpoint != null)
        {
            introCheckpoint.Activate();
        }


        StartTripleBFollowing(true);
    }



    private void StartTripleBFollowing(bool waitUntilReached = false)
    {
        if (!FindReferences())
            return;

        if (waitUntilReached)
        {
            waitingForTripleB = true;

            tripleBFollow.OnReachedPlayer -= OnTripleBReachedPlayer;
            tripleBFollow.OnReachedPlayer += OnTripleBReachedPlayer;
        }

        tripleBFollow.StartFollowing();
    }


    private void OnTripleBReachedPlayer()
    {
        if (!waitingForTripleB)
            return;

        waitingForTripleB = false;

        tripleBFollow.OnReachedPlayer -= OnTripleBReachedPlayer;

        StartTutorialDialogue();
    }


    private void StartTutorialDialogue()
    {
        if (!FindReferences())
            return;

        bool dialogueStarted =
            dialougeManager.OpenDialouge(tutorialMessages,actors);

        if (!dialogueStarted)
            return;

        dialougeManager.OnDialougeFinished += OnTutorialDialogueFinished;
    }


    private void OnTutorialDialogueFinished()
    {
        dialougeManager.OnDialougeFinished -= OnTutorialDialogueFinished;


        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.MarkDialogueCompleted(tutorialDialogueID);

            CheckpointManager.Instance.SetTripleBStage( CheckpointManager.TripleBStage.LaserTutorialFinished);
        }


        if (laserTutorialCheckpoint != null)
        {
            laserTutorialCheckpoint.Activate();
        }


        StartWaitingForLaser();
    }



    private void StartWaitingForLaser()
    {
        if (!FindReferences())
            return;

        waitingForLaser = true;

        tripleBLaser.OnLaserFinished -= OnLaserFinished;

        tripleBLaser.OnLaserFinished += OnLaserFinished;
    }


    private void OnLaserFinished()
    {
        if (!waitingForLaser)
            return;

        waitingForLaser = false;

        tripleBLaser.OnLaserFinished -= OnLaserFinished;



        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SetTripleBStage(CheckpointManager.TripleBStage.LaserUsed);
        }

        if (laserUsedCheckpoint != null)
        {
            laserUsedCheckpoint.Activate();
        }


        StartAfterLaserDialogue();
    }



    private void StartAfterLaserDialogue()
    {
        if (!FindReferences())
            return;

        finishingTutorial = true;

        bool dialogueStarted = dialougeManager.OpenDialouge( afterLaserMessages, actors);

        if (!dialogueStarted)
        {
            FinishTripleBTutorial();
            return;
        }

        dialougeManager.OnDialougeFinished += OnAfterLaserDialogueFinished;
    }


    private void OnAfterLaserDialogueFinished()
    {
        dialougeManager.OnDialougeFinished -= OnAfterLaserDialogueFinished;


        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.MarkDialogueCompleted(afterLaserDialogueID);

            CheckpointManager.Instance.SetTripleBStage(CheckpointManager.TripleBStage.Completed
            );
        }


        FinishTripleBTutorial();
    }



    private void FinishTripleBTutorial()
    {
        if (!finishingTutorial)
            return;

        finishingTutorial = false;

        if (rotTimer != null)
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
            return false;


        if (tripleBLaser == null)
            return false;
 


        if (dialougeManager == null)
            return false;


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
            tripleBFollow.OnReachedPlayer -=  OnTripleBReachedPlayer;
        }


        if (tripleBLaser != null)
        {
            tripleBLaser.OnLaserFinished -=  OnLaserFinished;
        }
    }
}