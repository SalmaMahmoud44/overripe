using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PeachBossShieldUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image shieldIcon;
    [SerializeField] private Image healthFill;

    [Header("Colors")]
    [SerializeField] private Color shieldedColor = Color.white;
    [SerializeField] private Color vulnerableColor = Color.white;

    [Header("Shield Animation")]
    [SerializeField] private float breakDuration = 0.25f;
    [SerializeField] private float shieldPunchScale = 1.25f;

    private Vector3 originalScale;
    private Coroutine shieldRoutine;

    private void Awake()
    {
        if (shieldIcon != null)
        {
            originalScale = shieldIcon.transform.localScale;
            shieldIcon.gameObject.SetActive(false);
        }
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
    }

    public void BreakShield()
    {
        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldRoutine = StartCoroutine(BreakShieldRoutine());
    }

    private IEnumerator BreakShieldRoutine()
    {
        if (shieldIcon != null)
        {
            shieldIcon.gameObject.SetActive(true);

            float t = 0f;
            float expandDuration = breakDuration * 0.4f;
            float disappearDuration = breakDuration * 0.6f;

            while (t < expandDuration)
            {
                t += Time.deltaTime;

                float normalized = Mathf.Clamp01(t / expandDuration);

                float scale = Mathf.Lerp( 1f,shieldPunchScale,normalized);

                shieldIcon.transform.localScale =originalScale * scale;

                yield return null;
            }
            t = 0f;

            while (t < disappearDuration)
            {
                t += Time.deltaTime;

                float normalized = Mathf.Clamp01(t / disappearDuration);

                float scale = Mathf.Lerp( shieldPunchScale, 0f,normalized);

                shieldIcon.transform.localScale = originalScale * scale;

                yield return null;
            }

            shieldIcon.gameObject.SetActive(false);
        }

        if (healthFill != null)
            healthFill.color = vulnerableColor;

        shieldRoutine = null;
    }

    public void RestoreShield()
    {
        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        shieldRoutine = StartCoroutine(RestoreShieldRoutine());
    }

    private IEnumerator RestoreShieldRoutine()
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

                float normalized = Mathf.Clamp01(t / duration);

                float scale = Mathf.Lerp(0f, 1f, normalized);

                shieldIcon.transform.localScale =originalScale * scale;

                yield return null;
            }

            shieldIcon.transform.localScale = originalScale;
        }

        if (healthFill != null)
            healthFill.color = shieldedColor;

        shieldRoutine = null;
    }
}