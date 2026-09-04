using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private DialougeTrigger timerTurorialTrigger;

    private void OnEnable()
    {
        enemyHealth.OnEnemyDied += HandleFirstKill; 
    }
    private void OnDisable()
    {
        enemyHealth.OnEnemyDied -= HandleFirstKill;
    }

    void HandleFirstKill()
    {
        timerTurorialTrigger.StartDialouge();
        enemyHealth.OnEnemyDied -= HandleFirstKill;
    }

    public void SkipTutorial()
    {
        SceneManager.LoadScene("Mango");
    }

}

