using System.Collections;
using UnityEngine;

public class AppleBossVulnerable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] BossHealth bossHealth;

    [Header("Vulnerable Settings")]
    [SerializeField] float vulnerableDuration = 3f;

    [Header("Animator")]
    [SerializeField] Animator bodyAnimator;
    [SerializeField] string tiredTrigger = "tired";
    [SerializeField] string tiredBool = "isTired";

    [Header("Visual")]
    [SerializeField] VulnerableOutline vulnerableOutline;
    [SerializeField] AppleBossShieldUI shieldUI;

    bool isVulnerable;
    Coroutine vulnerableRoutine;

    public bool IsVulnerable => isVulnerable;

    private void Awake()
    {
        if (bossHealth == null)
            bossHealth = GetComponent<BossHealth>();

        if (bodyAnimator == null)
            bodyAnimator = GetComponentInChildren<Animator>();
    }

    public void StartTired()
    {
        if (isVulnerable)
            return;

        if (bossHealth == null)
            return;

        if (bodyAnimator != null)
        {
            bodyAnimator.speed = 1f;

            bodyAnimator.ResetTrigger(tiredTrigger);
            bodyAnimator.SetTrigger(tiredTrigger);
        }
    }

    public void BecomeVulnerable()
    {
        if (isVulnerable)
            return;

        isVulnerable = true;

        if(bossHealth != null)
        bossHealth.SetVulnerable(true);

        if(shieldUI != null)
            shieldUI.BreakShield();

        if (bodyAnimator != null)
        {
            bodyAnimator.SetBool(tiredBool, true);
            bodyAnimator.speed = 0f;
        }

        SetVulnerableVisual(true);

        if (vulnerableRoutine != null)
            StopCoroutine(vulnerableRoutine);

        vulnerableRoutine = StartCoroutine(VulnerableRoutine());

    }

    private IEnumerator VulnerableRoutine()
    {
        yield return new WaitForSeconds(vulnerableDuration);

        EndVulnerable();
    }

    public void EndVulnerable()
    {
        if (!isVulnerable)
            return;

        isVulnerable = false;

        if (bossHealth != null)
            bossHealth.SetVulnerable(false);

        if (shieldUI != null)
            shieldUI.RestoreShield();

        SetVulnerableVisual(false);


        if (bodyAnimator != null)
        {
            bodyAnimator.speed = 1f;
            bodyAnimator.SetBool(tiredBool, false);
        }

        vulnerableRoutine = null;

    }

    private void SetVulnerableVisual(bool value)
    {
        if (vulnerableOutline != null)
        {
            if (value)
                vulnerableOutline.Show();
            else
                vulnerableOutline.Hide();
        }
    }
}
