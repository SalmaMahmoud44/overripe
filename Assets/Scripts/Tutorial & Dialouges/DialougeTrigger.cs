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

    private void Reset()
    {
        // Ensure the collider is set as a trigger
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
        Debug.Log("Starting Dialogue from Trigger: " + gameObject.name);

        if (dialougeManager.OpenDialouge(messages, actors))
        {
            Debug.Log("Dialogue started successfully.");
        }   
    
    }
}

public enum PlayerAction
{
    None,
    Move,
    Jump,
    Dash,
    Melee,
}
[System.Serializable]
public class Message
{
    public int actorId;
    public string message;  
    public PlayerAction waitForAction = PlayerAction.None;
}


[System.Serializable]
public class Actor
{
    public string name;
    public Sprite sprite;
}