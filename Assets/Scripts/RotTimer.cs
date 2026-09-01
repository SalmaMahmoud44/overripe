using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RotTimer : MonoBehaviour
{
    [SerializeField] int startTime = 60;
    [SerializeField] Image fillBar;
    [SerializeField] TextMeshProUGUI timerText;

    int currentTime;
    float tickTimer;

    void Start()
    {
        currentTime = startTime;
        UpdateUI();
    }

    void Update()
    {
        if (currentTime <= 0)
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

    public void AddTime(int amount)
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