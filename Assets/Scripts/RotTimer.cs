using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RotTimer : MonoBehaviour
{
    [SerializeField] int startTime = 60;
    [SerializeField] Image fillBar;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] bool sartPaused = false;


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
            UpdateUI();
        }
    }
    
    public void StartTimer()
    {
        timerRunning = true;
    }

    public void AddTime(float amount)
    {
        currentTime += amount;
        if (currentTime > startTime)
            currentTime = startTime; 
        UpdateUI();
    }

    void UpdateUI()
    {
        fillBar.fillAmount = (float)currentTime / startTime;
        timerText.text = currentTime.ToString();
    }
}