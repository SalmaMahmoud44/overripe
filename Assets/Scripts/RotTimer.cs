using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RotTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] int startTime = 60;
    [SerializeField] Image fillBar;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] bool sartPaused = false;

    [Header("Timer Audio Clip")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip timerWarningClip;

    bool isWarningTime = false;

    public float currentTime{ get; private set; }    
    public float NormalizedTime {
        get { return currentTime / startTime; }
    }


    float tickTimer;
    bool timerRunning;

    void Start()
    {
        currentTime = startTime;
        timerRunning = !sartPaused;
        UpdateUI();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        
    }

    void Update()
    {
        if (!timerRunning || currentTime <= 0)
            return;

        tickTimer += Time.deltaTime;
        if (tickTimer >= 1f)
        {
            tickTimer -= 1f;
            currentTime--;
            if (currentTime < 0)
                currentTime = 0;

            if (currentTime <= 10 && isWarningTime)
            {
                audioSource.PlayOneShot(timerWarningClip);
                isWarningTime = false; 
            }

            UpdateUI();
        }
    }
    
    public void StartTimer()
    {
        timerRunning = true;
    }
    public void PauseTimer()
    {
        timerRunning = false;
    }
    public void ResumeTimer()
    {
        timerRunning = true;
    }

    public void AddTime(float amount)
    {
        currentTime += amount;
        if (currentTime > startTime)
            currentTime = startTime;

        if (currentTime > 10)
            isWarningTime = true;
        UpdateUI();
    }

    void UpdateUI()
    {
        fillBar.fillAmount = (float)currentTime / startTime;
        timerText.text = currentTime.ToString();
    }
}