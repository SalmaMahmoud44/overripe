using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] BossHealth bossHealth;
    [SerializeField] Slider slider;
    [SerializeField] CanvasGroup canvasGroup;

    [Header("Fade In/Out")]
    [SerializeField] float fadeDuration = 0.4f;

    [SerializeField] bool smoothFill = true;
    [SerializeField] float fillSpeed = 3f;

    Coroutine fadeRoutine;
    Coroutine fillRoutine;
    float targetValue = 1f;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.OnDamaged -= HandleDamaged;
    }

    void HandleDamaged(float healthPercent)
    {
        targetValue = healthPercent;

        if (!smoothFill)
        {
            if (slider != null) slider.value = targetValue;
            return;
        }

        if (fillRoutine != null) StopCoroutine(fillRoutine);
        fillRoutine = StartCoroutine(SmoothFillRoutine());
    }

    IEnumerator SmoothFillRoutine()
    {
        while (slider != null && !Mathf.Approximately(slider.value, targetValue))
        {
            slider.value = Mathf.MoveTowards(slider.value, targetValue, fillSpeed * Time.deltaTime);
            yield return null;
        }
        fillRoutine = null;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (bossHealth != null && slider != null)
        {
            targetValue = bossHealth.GetHealthPercent();
            slider.value = targetValue; 
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(0f, disableAfter: true));
    }

    IEnumerator FadeTo(float target, bool disableAfter = false)
    {
        if (canvasGroup == null) yield break;

        float start = canvasGroup.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = target;

        if (disableAfter)
            gameObject.SetActive(false);
    }
}
