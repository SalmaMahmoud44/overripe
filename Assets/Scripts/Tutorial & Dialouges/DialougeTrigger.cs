using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class DialougeTrigger : MonoBehaviour
{
    [Header("Dialouge Settings")]
    [SerializeField] private Message[] messages;
    [SerializeField] private Actor[] actors;
    [SerializeField] DialougeManager dialougeManager;
    [SerializeField] private string playerTag = "Player";

    bool hasTriggered = false;

    [Header("Timer Settings")]
    [SerializeField] private bool startTimerAfterDialogue = false;
    [SerializeField] private RotTimer rotTimer;


    private void Reset()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag(playerTag))
        {
            hasTriggered = true;
            StartDialouge();
        }
    }
    public void StartDialouge()
    {
      
        if (dialougeManager == null)
        {
            dialougeManager = GameObject.Find("DialougeManager")
                .GetComponent<DialougeManager>();
        }

        if (rotTimer == null)
        {
            rotTimer = GameObject.Find("RotTimerCanvas")
                .GetComponent<RotTimer>();
        }

        Debug.Log("Starting Dialogue from Trigger: " + gameObject.name);

        if (dialougeManager.OpenDialouge(messages, actors))
        {
            Debug.Log("Dialogue started successfully.");

            if (startTimerAfterDialogue)
            {
                dialougeManager.OnDialougeFinished += StartTimerAfterDialogue;
            }
        }   

        void StartTimerAfterDialogue()
        {
            dialougeManager.OnDialougeFinished -= StartTimerAfterDialogue;
            if (rotTimer != null)
            {
                rotTimer.StartTimer();
            }
        }

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