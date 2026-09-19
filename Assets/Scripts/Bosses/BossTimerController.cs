using UnityEngine;

public class BossTimerStopper : MonoBehaviour
{
    [SerializeField] RotTimer rotTimer;
    [SerializeField] MonoBehaviour bossReference;

    private IBoss boss;

    private void Awake()
    {
        boss = bossReference as IBoss;
    }

    private void Start()
    {
        if (boss != null)
            boss.OnBossDied += StopTimer;
    }

    private void OnDestroy()
    {
        if (boss != null)
            boss.OnBossDied -= StopTimer;
    }

    private void StopTimer()
    {
        rotTimer.PauseTimer();
    }
}