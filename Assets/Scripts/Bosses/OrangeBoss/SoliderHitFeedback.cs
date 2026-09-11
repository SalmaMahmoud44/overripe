using System.Collections;
using UnityEngine;

public class SoliderHitFeedback : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Color hitColor = Color.white;
    [SerializeField] float flashDuration = 0.15f;
    [SerializeField] float stunDuration = 0.35f;

    Color baseColor;
    Coroutine routine;

    public bool CanAct { get; private set; } = true;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    public void PlayHitFeedback()
    {
        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(FeedbackRoutine());
    }

    IEnumerator FeedbackRoutine()
    {
        CanAct = false;

        if (spriteRenderer != null)
            spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(flashDuration);

        if (spriteRenderer != null)
            spriteRenderer.color = baseColor;

        float remaining = stunDuration - flashDuration;
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        CanAct = true;
        routine = null;
    }
}
