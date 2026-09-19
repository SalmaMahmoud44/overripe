using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    private string currentScene;

    private string checkpointID;
    private Vector3 checkpointPosition;
    private bool hasCheckpoint;

    private HashSet<string> completedDialogues = new HashSet<string>();

    public enum TripleBStage
    {
        None,
        IntroFinished,
        LaserTutorialFinished,
        LaserUsed,
        Completed
    }

    private TripleBStage tripleBStage = TripleBStage.None;

    public TripleBStage CurrentTripleBStage => tripleBStage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentScene = SceneManager.GetActiveScene().name;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetCheckpoint(string id, Vector3 position)
    {
        checkpointID = id;
        checkpointPosition = position;
        hasCheckpoint = true;

        currentScene = SceneManager.GetActiveScene().name;
    }

    public bool HasCheckpoint()
    {
        return hasCheckpoint && currentScene == SceneManager.GetActiveScene().name;
    }

    public Vector3 GetCheckpointPosition()
    {
        return checkpointPosition;
    }

    public string GetCheckpointID()
    {
        return checkpointID;
    }


    public void MarkDialogueCompleted(string dialogueID)
    {
        if (string.IsNullOrEmpty(dialogueID))
            return;
        completedDialogues.Add(dialogueID);

    }

    public bool IsDialogueCompleted(string dialogueID)
    {
        if (string.IsNullOrEmpty(dialogueID))
            return false;

        return completedDialogues.Contains(dialogueID);
    }


    public void SetTripleBStage(TripleBStage stage)
    {
        tripleBStage = stage;

        Debug.Log($"Triple B Stage: {stage}");
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != currentScene)
        {
            ClearProgress();

            currentScene = scene.name;

            return;
        }

        if (hasCheckpoint)
        {
            StartCoroutine(RestorePlayerAtCheckpoint());
        }
    }

    private IEnumerator RestorePlayerAtCheckpoint()
    {
        yield return null;

        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player == null)
            yield break;

        player.transform.position = checkpointPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }


    private void ClearProgress()
    {
        checkpointID = null;
        checkpointPosition = Vector3.zero;
        hasCheckpoint = false;

        completedDialogues.Clear();

        tripleBStage = TripleBStage.None;
    }
}