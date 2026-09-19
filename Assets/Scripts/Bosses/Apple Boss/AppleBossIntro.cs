using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class AppleBossIntro : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialougeManager dialogueManager;

    [SerializeField] private DialougeTrigger dialogue1;
    [SerializeField] private DialougeTrigger dialogue2;
    [SerializeField] private DialougeTrigger dialogue3;

    [Header("Player")]
    [SerializeField] private PlayerController player;

    [Header("Boss")]
    [SerializeField] private AppleBossController bossController;
    [SerializeField] private BossHealthUI bossHealthUI;

    [Header("Normal Apple")]
    [SerializeField] private GameObject normalApple;

    [Header("Cage Intro")]
    [SerializeField] private GameObject cageIntro;

    [Header("Cage Intro Parts")]
    [SerializeField] private Transform tripleB;
    [SerializeField] private Transform cage;

    [Header("Cage Movement")]
    [SerializeField] private Transform cageStartPoint;
    [SerializeField] private Transform cageDownPoint;
    [SerializeField] private Transform cageUpPoint;

    [SerializeField] private float cageMoveSpeed = 5f;

    [Header("Fusion Movement")]
    [SerializeField] private Transform appleFusionPoint;
    [SerializeField] private Transform tripleBFusionPoint;

    [SerializeField] private float flySpeed = 8f;
    [SerializeField] private float beforeCollisionDelay = 0.15f;

    [Header("Explosion")]
    [SerializeField] private GameObject explosion;
    [SerializeField] private float explosionDuration = 1f;

    [Header("Final Boss")]
    [SerializeField] private GameObject finalBoss;

    [Header("Timing")]
    [SerializeField] private float afterCageDownDelay = 0.3f;
    [SerializeField] private float afterCageUpDelay = 0.2f;
    [SerializeField] private float afterExplosionDelay = 0.15f;
    [SerializeField] private float beforeFightDelay = 0.2f;

    [Header("Explosion Camera Shake")]
    [SerializeField] private CinemachineImpulseSource explosionImpulse;

    private bool introStarted;
    private bool introFinished;

    private void Awake()
    {
        PrepareIntro();
    }

    private void PrepareIntro()
    {
   
        if (normalApple != null)
            normalApple.SetActive(true);

        if (cageIntro != null)
            cageIntro.SetActive(false);

        if (tripleB != null)
            tripleB.gameObject.SetActive(true);

        if (cage != null)
            cage.gameObject.SetActive(true);

        if (explosion != null)
            explosion.SetActive(false);

        if (finalBoss != null)
            finalBoss.SetActive(false);
    }

    public void StartIntro()
    {
        if (introStarted || introFinished)
            return;

        introStarted = true;

        if (cageIntro != null)
        {
            cageIntro.SetActive(true);

            if (cageStartPoint != null)
                cageIntro.transform.position = cageStartPoint.position;
        }

        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        if (dialogueManager == null)
        {
            Debug.LogError("AppleBossIntro: Dialogue Manager is not assigned.");
            yield break;
        }

        if (dialogue1 == null)
        {
            Debug.LogError("AppleBossIntro: Dialogue 1 is not assigned.");
            yield break;
        }

        yield return PlayDialogueAndWait(dialogue1);



        yield return MoveCage(cageDownPoint);

        yield return new WaitForSeconds(afterCageDownDelay);


        if (dialogue2 != null)
            yield return PlayDialogueAndWait(dialogue2);


        if (dialogue3 != null)
            yield return PlayDialogueAndWait(dialogue3);

        if (player != null) 
            player.SetControlsLocked(true);

        if (tripleB != null)
            tripleB.SetParent(null);

        if (cage != null)
            cage.SetParent(null);



        Coroutine cageRoutine = StartCoroutine(MoveTransform(cage,cageUpPoint,cageMoveSpeed));

        Coroutine tripleBRoutine = StartCoroutine(MoveTransform(tripleB, tripleBFusionPoint, flySpeed));

        yield return cageRoutine;
        yield return tripleBRoutine;

        yield return new WaitForSeconds(afterCageUpDelay);

        yield return FusionMovement();


        yield return ExplosionAndBossReveal();

        introFinished = true;
    }


    private IEnumerator PlayDialogueAndWait(DialougeTrigger dialogueTrigger)
    {
        bool finished = false;

        void OnDialogueFinished()
        {
            finished = true;
        }

        dialogueManager.OnDialougeFinished += OnDialogueFinished;

        dialogueTrigger.StartDialouge();

        yield return new WaitUntil(() => finished);

        dialogueManager.OnDialougeFinished -= OnDialogueFinished;
    }


    private IEnumerator MoveCage(Transform targetPoint)
    {
        if (cageIntro == null || targetPoint == null)
            yield break;

        while ( Vector3.Distance( cageIntro.transform.position,targetPoint.position ) > 0.02f)
        {
            cageIntro.transform.position = Vector3.MoveTowards( cageIntro.transform.position, targetPoint.position, cageMoveSpeed * Time.deltaTime);

            yield return null;
        }

        cageIntro.transform.position = targetPoint.position;
    }


    private IEnumerator MoveTransform(Transform objectToMove,Transform targetPoint,float speed)
    {
        if (objectToMove == null || targetPoint == null)
            yield break;

        while (Vector3.Distance( objectToMove.position,targetPoint.position ) > 0.02f)
        {
            objectToMove.position = Vector3.MoveTowards( objectToMove.position,  targetPoint.position, speed * Time.deltaTime );

            yield return null;
        }

        objectToMove.position = targetPoint.position;
    }

    private IEnumerator FusionMovement()
    {
        if (normalApple == null)
        {
            Debug.LogError("AppleBossIntro: Normal Apple is missing.");
            yield break;
        }

        if (tripleB == null)
        {
            Debug.LogError("AppleBossIntro: Triple B is missing.");
            yield break;
        }

        Vector3 appleTarget = appleFusionPoint != null ? appleFusionPoint.position: normalApple.transform.position;

        Vector3 tripleBTarget =tripleBFusionPoint != null ? tripleBFusionPoint.position : tripleB.position;

        bool appleArrived = false;
        bool tripleBArrived = false;

        while (!appleArrived || !tripleBArrived)
        {
            if (!appleArrived)
            {
                normalApple.transform.position = Vector3.MoveTowards(normalApple.transform.position,appleTarget, flySpeed * Time.deltaTime);

                appleArrived = Vector3.Distance( normalApple.transform.position, appleTarget ) <= 0.02f;
            }

            if (!tripleBArrived)
            {
                tripleB.position = Vector3.MoveTowards( tripleB.position, tripleBTarget, flySpeed * Time.deltaTime );

                tripleBArrived = Vector3.Distance( tripleB.position, tripleBTarget ) <= 0.02f;
            }

            yield return null;
        }

        normalApple.transform.position = appleTarget;
        tripleB.position = tripleBTarget;

        yield return new WaitForSeconds(beforeCollisionDelay);
    }

    private IEnumerator ExplosionAndBossReveal()
    {

        if (normalApple != null)
            normalApple.SetActive(false);

        if (tripleB != null)
            tripleB.gameObject.SetActive(false);

        if (cage != null)
            cage.gameObject.SetActive(false);

        if (explosion != null)
        {
            explosion.transform.position =appleFusionPoint != null? appleFusionPoint.position: transform.position;

            explosion.SetActive(true);

            if (explosionImpulse != null)
                explosionImpulse.GenerateImpulse();

            yield return new WaitForSeconds(explosionDuration);

            explosion.SetActive(false);
        }

        if (finalBoss != null)
            finalBoss.SetActive(true);

        yield return new WaitForSeconds(afterExplosionDelay);

        if (bossHealthUI != null)
            bossHealthUI.Show();

        yield return new WaitForSeconds(beforeFightDelay);

        if (bossController != null)
            bossController.StartFight();

        if (player != null) 
            player.SetControlsLocked(false);

    }
}
