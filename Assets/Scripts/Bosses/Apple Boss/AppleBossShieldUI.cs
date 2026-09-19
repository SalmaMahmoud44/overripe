using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AppleBossShieldUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image shieldIcon;
    [SerializeField] Image healthFill;

    [Header("Colors")]
    [SerializeField] Color shieldedColor = Color.white;
    [SerializeField] Color vulnerableColor = Color.white;

    [Header("Shield Animation")]
    [SerializeField] float breakDuration = 0.25f;
    [SerializeField] float shieldPunchScale = 1.25f;

    [Header("Boss Icon")]
    [SerializeField] GameObject bossIconShielded;
    [SerializeField] GameObject bossIconVulnerable;

    Vector3 originalScale;
    Coroutine shieldRoutine;

    private void Awake()
    {
        if (shieldIcon != null)
            originalScale = shieldIcon.transform.localScale;

        SetShieldedInstant();
    }

    public void SetShieldedInstant()
    {
        if (shieldIcon != null)
        {
            shieldIcon.gameObject.SetActive(true);
            shieldIcon.transform.localScale = originalScale;
        }

        if (healthFill != null)
            healthFill.color = shieldedColor;

        SetBossIcon(false);
    }

    public void BreakShield()
    {
        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldRoutine = StartCoroutine(BreakShieldRoutine());
    }

    IEnumerator BreakShieldRoutine()
    {
        if (shieldIcon != null)
        {
            shieldIcon.gameObject.SetActive(true);

            float t = 0f;

            while (t < breakDuration * 0.4f)
            {
                t += Time.deltaTime;

                float normalized = t / (breakDuration * 0.4f);

                float scale = Mathf.Lerp(1f, shieldPunchScale, normalized);

                shieldIcon.transform.localScale = originalScale * scale;

                yield return null;
            }


            t = 0f;

            while (t < breakDuration * 0.6f)
            {
                t += Time.deltaTime;

                float normalized = t / (breakDuration * 0.6f);

                float scale = Mathf.Lerp(shieldPunchScale, 0f, normalized);

                shieldIcon.transform.localScale = originalScale * scale;

                yield return null;
            }

            shieldIcon.gameObject.SetActive(false);
        }

        if (healthFill != null)
            healthFill.color = vulnerableColor;

        SetBossIcon(true);

        shieldRoutine = null;
    }

    public void RestoreShield()
    {
        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldRoutine = StartCoroutine(RestoreShieldRoutine());
    }

    IEnumerator RestoreShieldRoutine()
    {
        if (shieldIcon != null)
        {
            shieldIcon.gameObject.SetActive(true);
            shieldIcon.transform.localScale = Vector3.zero;

            float duration = 0.25f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;

                float normalized = t / duration;

                float scale = Mathf.Lerp(0f, 1f, normalized);

                shieldIcon.transform.localScale = originalScale * scale;

                yield return null;
            }

            shieldIcon.transform.localScale = originalScale;
        }

        if (healthFill != null)
            healthFill.color = shieldedColor;

        SetBossIcon(false);

        shieldRoutine = null;
    }

    void SetBossIcon(bool vulnerable)
    {
        if (bossIconShielded != null)
            bossIconShielded.SetActive(!vulnerable);

        if (bossIconVulnerable != null)
            bossIconVulnerable.SetActive(vulnerable);
    }
}
