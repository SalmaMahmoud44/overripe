using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using System.Collections;

public class DialougeManager : MonoBehaviour
{
    [Header("Dialouge UI Elements")]
    [SerializeField] Image actorImage;
    [SerializeField] TextMeshProUGUI actorName;
    [SerializeField] TextMeshProUGUI dialouge;
    [SerializeField] RectTransform dialougeBox;

    [Header("Player Reference")]
    [SerializeField] PlayerController player;

    [Header("Dialouge Timing Settings")]
    [SerializeField] float baseDisplayTime = 1f; // Base time to display each message
    [SerializeField] float timePerCharacter = 0.05f; // Additional time per character in the message

    [Header("Key Prompt UI")]   
    [SerializeField] KeyPromptUI keyPromptUI;

    [Header("Timer Reference")]
    [SerializeField] RotTimer rotTimer;
    

    PlayerAction currentWaitAction;
    Message[] currentMessage;
    Actor[] currentActor;

    int activeMessage = 0;

    Coroutine autoAdvanceRoutine;

    bool pressedA;
    bool pressedD;

   private bool isActive = false;
    public event Action OnDialougeFinished;

    private void Awake()
    {
        isActive = false;

        if(rotTimer == null)
            rotTimer = GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();
    }
    public bool OpenDialouge(Message[] message, Actor[] actor)
    {
        Debug.Log($"OpenDialogue called | isActive = {isActive}");
        if (isActive)
        {
            Debug.LogWarning("Dialogue is already active.");
            return false;
        }

        currentMessage = message;
        currentActor = actor;
        activeMessage = 0;
        isActive = true;

        if(rotTimer != null)
            rotTimer.PauseTimer();

        dialougeBox.gameObject.SetActive(true);
        DisplayMessage();

        return true;
    }

    void DisplayMessage()
    {
        pressedA = false;
        pressedD = false;

        Message messageToDisplay = currentMessage[activeMessage];
        dialouge.text = messageToDisplay.message;

        Actor actorToDisplay = currentActor[messageToDisplay.actorId];
        actorImage.sprite = actorToDisplay.sprite;
        actorName.text = actorToDisplay.name;

        keyPromptUI.ShowPrompt(messageToDisplay.waitForAction);

        if (autoAdvanceRoutine != null)
            StopCoroutine(autoAdvanceRoutine);
        UnSubscribeAll();

        currentWaitAction = messageToDisplay.waitForAction;

        if (currentWaitAction == PlayerAction.None)
        {
            player.SetControlsLocked(true);
            autoAdvanceRoutine = StartCoroutine(AutoAdvance(messageToDisplay.message));
        }
        else
        {
            player.SetControlsLocked(false);
            SubscribeToPlayerAction(messageToDisplay.waitForAction);
        }

    }
    IEnumerator AutoAdvance(string text)
    {
        float duration = baseDisplayTime + text.Length * timePerCharacter;
        yield return new WaitForSeconds(duration);
        NextMessage();
    }
    IEnumerator AdvanceAfterLight()
    {
        yield return new WaitForSeconds(0.5f); // Wait for the light-up effect to be noticeable
        NextMessage();
    }
    void SubscribeToPlayerAction(PlayerAction action)
    {

        switch (action)
        {
            case PlayerAction.Move:
                player.OnPlayerMoved += OnMovePerformed;
                break;
            case PlayerAction.Jump:
                player.OnPlayerJumped += OnActionPerformed;
                break;
            case PlayerAction.Dash:
                player.OnPlayerDashed += OnActionPerformed;
                break;
            case PlayerAction.Melee:
                player.OnPlayerMelee += OnActionPerformed;
                break;
        }
    } 
    void UnSubscribeAll()
    {
        player.OnPlayerMoved -= OnMovePerformed;
        player.OnPlayerJumped -= OnActionPerformed;
        player.OnPlayerDashed -= OnActionPerformed;
        player.OnPlayerMelee -= OnActionPerformed;
    }
    void OnMovePerformed(KeyCode key)
    {
        if (key == KeyCode.A)
        {
            pressedA = true;
            keyPromptUI.LightUpKey(KeyCode.A);
        }

        if (key == KeyCode.D)
        {
            pressedD = true;
            keyPromptUI.LightUpKey(KeyCode.D);
        }
        if (pressedA || pressedD)
        {
            UnSubscribeAll();
            StartCoroutine(AdvanceAfterLight());
        }

    }
    void OnActionPerformed()
    {
        keyPromptUI.LightUpAction(currentWaitAction);
        UnSubscribeAll();
        StartCoroutine(AdvanceAfterLight());
    }
    public void NextMessage()
    {
        activeMessage++;
        if (activeMessage < currentMessage.Length)
        {
            DisplayMessage();
        }
        else
        {
            Debug.Log("End of Dialouge");
            isActive = false;

            player.SetControlsLocked(false);

            dialougeBox.gameObject.SetActive(false);
            keyPromptUI.Hide();

            //if (rotTimer != null && resumeTimer)
            //    rotTimer.ResumeTimer();


            OnDialougeFinished?.Invoke();
        }
    }
}
