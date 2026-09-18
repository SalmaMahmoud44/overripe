using System.Collections;
using UnityEngine;

public class AppleBossStickAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] AppleBossController appleBoss;

    [SerializeField] GameObject normalArms;
    [SerializeField] GameObject stickArm;
    [SerializeField] GameObject emptyArm;

    [SerializeField] Animator stickArmAnimator;
    [SerializeField] Animator emptyArmAnimator;
    [SerializeField] Animator stickHitboxAnimator;

    [Header("Attack")]
    [SerializeField] AppleBossStickHitbox stickHitbox;

    [Header("Safety")]
    [SerializeField] float attackSafetyTime = 3f;

    [Header("Animation")]
    [SerializeField] string stickAnimTrigger = "stickAttack";



    Coroutine safetyCoroutine;

    bool attackActive;

    public bool IsAttacking => attackActive;

    private void Awake()
    {
        if (appleBoss == null)
            appleBoss = GetComponent<AppleBossController>();

        if (stickArmAnimator == null && stickArm != null)
            stickArmAnimator = stickArm.GetComponent<Animator>();

        if (emptyArmAnimator == null && emptyArm != null)
            emptyArmAnimator = emptyArm.GetComponent<Animator>();

        if (stickHitboxAnimator == null && stickHitbox != null)
            stickHitboxAnimator = stickHitbox.GetComponent<Animator>();

    }

    public void StartAttack()
    {
        if (attackActive)
            return;

        if (appleBoss == null || appleBoss.Player == null)
            return;

        attackActive = true;

        SetArmsForStick();


        StartSafetyTimer();


        if (appleBoss.BodyAnimator != null)
        {
            appleBoss.BodyAnimator.ResetTrigger(stickAnimTrigger);
            appleBoss.BodyAnimator.SetTrigger(stickAnimTrigger);
        }

        if (stickArmAnimator != null)
        {
            stickArmAnimator.ResetTrigger(stickAnimTrigger);
            stickArmAnimator.SetTrigger(stickAnimTrigger);
        }

        if (emptyArmAnimator != null)
        {
            emptyArmAnimator.ResetTrigger(stickAnimTrigger);
            emptyArmAnimator.SetTrigger(stickAnimTrigger);

        }

        if (stickHitboxAnimator != null)
        {
            stickHitboxAnimator.ResetTrigger(stickAnimTrigger);
            stickHitboxAnimator.SetTrigger(stickAnimTrigger);
        }
    }

    void StartSafetyTimer()
    {
        StopSafetyTimer();

        safetyCoroutine = StartCoroutine(SafetyTimerRoutine());
    }

    IEnumerator SafetyTimerRoutine()
    {
        yield return new WaitForSeconds(attackSafetyTime);

        if (attackActive)
        {
            FinishAttack();
        }

        safetyCoroutine = null;
    }
    void StopSafetyTimer()
    {
        if (safetyCoroutine != null)
        {
            StopCoroutine(safetyCoroutine);
            safetyCoroutine = null;
        }
    }
    void SetArmsForStick()
    {
        normalArms.SetActive(false);
        stickArm.SetActive(true);
        emptyArm.SetActive(true);
    }

    public void OnStickHitboxOn()
    {

        if (!attackActive)

            return;

        if (stickHitbox == null)

            return;

        stickHitbox.Activate();
    }

    public void OnStaffHitboxOff()
    {

        if (stickHitbox != null)
            stickHitbox.Deactivate();
    }

    public void FinishAttack()
    {
        if (!attackActive)
            return;

        attackActive = false;

        StopSafetyTimer();

        if (stickHitbox != null)
            stickHitbox.Deactivate();

        ResetArms();

        Debug.Log("Apple Staff Attack Finished");
    }

    void ResetArms()
    {
        normalArms.SetActive(true);
        stickArm.SetActive(false);
        emptyArm.SetActive(false);
    }




}
