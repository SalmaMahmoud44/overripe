using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppleBossController : MonoBehaviour,IBoss
{
    public enum BossState{ Idle,Moving, Attacking, Vulnerable,Dead}

    public enum BossPhase{Phase1,Phase2,Phase3}

    public enum AttackType {  Seed,Stick}


    [Header("References")]
    [SerializeField] Transform player;

    [SerializeField] AppleBossSeedAttack seedAttack;
    [SerializeField] AppleBossMovement bossMovement;
    [SerializeField] AppleBossStickAttack stickAttack;
    [SerializeField] AppleBossVulnerable vulnerable;
    [SerializeField] BossHealth health;


    [Header("Animator")]
    [SerializeField] Animator bodyAnimator;
    [SerializeField] Animator armAnimator;
    [SerializeField] Animator stickArmAnimator;
    [SerializeField] Animator emptyArmAnimator;


    [Header("Fight Timing")]
    [SerializeField] float delayBetweenActions = 0.3f;

    [SerializeField][Range(0f, 1f)] float moveChance = 0.7f;
    [SerializeField][Range(0f, 1f)] float attackWhileMovingChance = 0.40f;


    [Header("Movement Position")]
    [SerializeField] Transform leftPosition;
    [SerializeField] Transform rightPosition;
    [SerializeField] Transform centerPosition;


    [Header("Phase Movement")]
    [SerializeField][Range(0f, 1f)] float phase1MoveChance = 0.65f;
    [SerializeField][Range(0f, 1f)] float phase2MoveChance = 0.70f;
    [SerializeField][Range(0f, 1f)] float phase3MoveChance = 0.80f;


    [Header("Phase Attacks")]
    [SerializeField] int phase1AttackCount = 3;
    [SerializeField] int phase2AttackCount = 4;
    [SerializeField] int phase3AttackCount = 5;


    [Header("Current Fight")]
    [SerializeField] BossPhase currentPhase = BossPhase.Phase1;


    [Header("Stick Decision")]
    [SerializeField] float stickMaxDistance = 6f;
    [SerializeField][Range(0f, 1f)] float stickChance = 0.65f;
    [SerializeField][Range(0f, 1f)] float farStickChance = 0.40f;

    [Header("Death")]
    [SerializeField] string deathTrigger = "Death";
    [SerializeField] float deathToCutsceneDelay = 1f;
    [SerializeField] string finalCutsceneScene = "FinalCutscene";


    public Transform Player => player;

    public Animator BodyAnimator => bodyAnimator;

    public Animator ArmAnimator => armAnimator;

    public AppleBossMovement BossMovement => bossMovement;

    public BossState CurrentState { get; private set; }

    public BossPhase CurrentPhase => currentPhase;



    Coroutine fightRoutine;

    int attacksPerformed;

    Transform lastPosition;

    AttackType lastAttack;

    bool hasLastPosition;
    bool hasLastAttack;

    public event System.Action OnBossDied;


    private void Awake()
    {
        if (seedAttack == null)
            seedAttack = GetComponent<AppleBossSeedAttack>();

        if (bossMovement == null)
            bossMovement = GetComponent<AppleBossMovement>();

        if (stickAttack == null)
            stickAttack = GetComponent<AppleBossStickAttack>();

        if (vulnerable == null)
            vulnerable = GetComponent<AppleBossVulnerable>();

        if (health == null)
            health = GetComponent<BossHealth>();
    }


    public void StartFight()
    {
        if (fightRoutine != null)
            StopCoroutine(fightRoutine);

        fightRoutine = StartCoroutine(FightRoutine());
    }



    private IEnumerator FightRoutine()
    {
        CurrentState = BossState.Idle;

        yield return new WaitForSeconds(1f);

        UpdatePhase();

        attacksPerformed = 0;


        while (CurrentState != BossState.Dead)
        {

            if (health != null && health.IsDead)
            {
                EnterDeadState();
                yield break;
            }

            UpdatePhase();


            CurrentState = BossState.Idle;

            yield return new WaitForSeconds(delayBetweenActions);


            AttackType attackType = GetNextAttack();


            Transform attackPosition = GetAttackPosition(attackType);

            bool forceMoveForStick = attackType == AttackType.Stick && !CanUseStick();


            bool shouldMove = attackPosition != null && (forceMoveForStick || ShouldMove());


            if (shouldMove)
            {
                CurrentState = BossState.Moving;

                bossMovement.MoveTo(attackPosition);

                if (UnityEngine.Random.value <= attackWhileMovingChance)
                {
                    CurrentState = BossState.Attacking;

                    StartAttack(attackType);


                    yield return new WaitUntil(() => !IsAnyAttackActive() || CurrentState == BossState.Dead);

                    if (health != null && health.IsDead)
                    {
                        EnterDeadState();
                        yield break;
                    }

                    attacksPerformed++;

                    if (ShouldBecomeVulnerable())
                    {
                        yield return EnterVulnerableState();

                        if (CurrentState == BossState.Dead)
                            yield break;

                        continue;
                    }

                    if (bossMovement != null)
                    {
                        yield return new WaitUntil(() => !bossMovement.IsMoving || CurrentState == BossState.Dead);
                    }

                    if (CurrentState == BossState.Dead)
                        yield break;

                    continue;
                }


                yield return new WaitUntil(() => !bossMovement.IsMoving || CurrentState == BossState.Dead);


                if (CurrentState == BossState.Dead)
                    yield break;
            }


            if (CurrentState == BossState.Vulnerable || CurrentState == BossState.Dead)
                continue;




            CurrentState = BossState.Attacking;

            StartAttack(attackType);


            yield return new WaitUntil(() => !IsAnyAttackActive() || CurrentState == BossState.Dead);


            if (health != null && health.IsDead)
            {
                EnterDeadState();
                yield break;
            }

            attacksPerformed++;


            if (ShouldBecomeVulnerable())
            {
                yield return EnterVulnerableState();

                if (CurrentState == BossState.Dead)
                    yield break;

                continue;
            }

            yield return new WaitForSeconds(delayBetweenActions);
        }
    }


    float GetCurrentMoveChance()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                return phase1MoveChance;

            case BossPhase.Phase2:
                return phase2MoveChance;

            case BossPhase.Phase3:
                return phase3MoveChance;
        }

        return moveChance;
    }


    bool ShouldMove()
    {
        return Random.value <= GetCurrentMoveChance();
    }


    private IEnumerator EnterVulnerableState()
    {
        StopCurrentActions();

        CurrentState = BossState.Vulnerable;

        ResetAttackTriggers();


        if (vulnerable != null)
        {
            vulnerable.StartTired();


            yield return new WaitUntil(() => vulnerable.IsVulnerable || CurrentState == BossState.Dead);


            if (CurrentState == BossState.Dead)
                yield break;


            yield return new WaitUntil(() => !vulnerable.IsVulnerable || CurrentState == BossState.Dead);
        }


        if (CurrentState == BossState.Dead)
            yield break;


        attacksPerformed = 0;

        hasLastAttack = false;
        hasLastPosition = false;


        UpdatePhase();

        CurrentState = BossState.Idle;
    }


    AttackType GetNextAttack()
    {

        if (currentPhase == BossPhase.Phase1)
        {
            if (!hasLastAttack)
            {
                lastAttack = GetSmartAttack();

                hasLastAttack = true;

                return lastAttack;
            }


            AttackType preferredAttack = lastAttack == AttackType.Seed ? AttackType.Stick : AttackType.Seed;


            lastAttack = preferredAttack;

            return preferredAttack;
        }


        AttackType selectedAttack = GetSmartAttack();

        lastAttack = selectedAttack;

        hasLastAttack = true;

        return selectedAttack;
    }


    bool CanUseStick()
    {
        if (player == null)
            return false;


        float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);


        return horizontalDistance <= stickMaxDistance;
    }


    AttackType GetSmartAttack()
    {

        if (CanUseStick())
        {
            if (UnityEngine.Random.value <= stickChance)
                return AttackType.Stick;

            return AttackType.Seed;
        }

        if (UnityEngine.Random.value <= farStickChance)
            return AttackType.Stick;

        return AttackType.Seed;
    }


    Transform GetAttackPosition(AttackType attackType)
    {
        if (bossMovement == null)
            return null;

        if (attackType == AttackType.Stick && player != null && !CanUseStick())
            return GetClosestPositionToPlayer();

        return GetRandomPosition();
    }


    Transform GetClosestPositionToPlayer()
    {
        Transform[] positions = GetValidPositions();


        if (positions.Length == 0 || player == null)
            return null;


        Transform closestPosition = positions[0];


        float closestDistance = Mathf.Abs(positions[0].position.x - player.position.x);


        for (int i = 1; i < positions.Length; i++)
        {
            float distance = Mathf.Abs(positions[i].position.x - player.position.x);


            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPosition = positions[i];
            }
        }


        lastPosition = closestPosition;

        hasLastPosition = true;


        return closestPosition;
    }


    Transform GetRandomPosition()
    {
        Transform[] positions =
            GetValidPositions();


        if (positions.Length == 0)
            return null;


        Transform selectedPosition;

        int attempts = 0;


        do
        {
            selectedPosition = positions[Random.Range(0, positions.Length)];
            attempts++;
        }
        while (hasLastPosition && selectedPosition == lastPosition && attempts < 10);


        lastPosition = selectedPosition;

        hasLastPosition = true;


        return selectedPosition;
    }

    Transform[] GetValidPositions()
    {
        int count = 0;

        if (leftPosition != null)
            count++;

        if (centerPosition != null)
            count++;

        if (rightPosition != null)
            count++;


        Transform[] positions = new Transform[count];

        int index = 0;


        if (leftPosition != null)
            positions[index++] = leftPosition;

        if (centerPosition != null)
            positions[index++] = centerPosition;

        if (rightPosition != null)
            positions[index++] = rightPosition;


        return positions;
    }

    void StartAttack(AttackType attackType)
    {
        if (CurrentState != BossState.Attacking)
            return;

        if (health != null && health.IsDead)
        {
            EnterDeadState();
            return;
        }

        switch (attackType)
        {
            case AttackType.Seed:

                StartSeedAttack();

                break;


            case AttackType.Stick:

                StartStickAttack();

                break;
        }
    }


    bool IsAnyAttackActive()
    {
        bool seedActive = seedAttack != null && seedAttack.IsAttacking;


        bool stickActive = stickAttack != null && stickAttack.IsAttacking;

        return seedActive || stickActive;
    }


    bool ShouldBecomeVulnerable()
    {
        return attacksPerformed >= GetCurrentAttackCount();
    }


    int GetCurrentAttackCount()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                return phase1AttackCount;

            case BossPhase.Phase2:
                return phase2AttackCount;

            case BossPhase.Phase3:
                return phase3AttackCount;
        }

        return phase1AttackCount;
    }


    void UpdatePhase()
    {
        if (health == null)
            return;

        float healthPercent = health.GetHealthPercent();

        BossPhase newPhase;

        if (healthPercent > 0.66f)
        {
            newPhase = BossPhase.Phase1;
        }
        else if (healthPercent > 0.33f)
        {
            newPhase = BossPhase.Phase2;
        }
        else
        {
            newPhase = BossPhase.Phase3;
        }


        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;

            hasLastAttack = false;
            hasLastPosition = false;

            attacksPerformed = 0;
        }
    }


    void ResetAttackTriggers()
    {
        if (bodyAnimator != null)
        {
            bodyAnimator.ResetTrigger("SeedFront");
            bodyAnimator.ResetTrigger("SeedRightClose");
            bodyAnimator.ResetTrigger("SeedRightFar");
            bodyAnimator.ResetTrigger("SeedLeftClose");
            bodyAnimator.ResetTrigger("SeedLeftFar");

            bodyAnimator.ResetTrigger("stickAttack");
        }


        if (armAnimator != null)
            armAnimator.ResetTrigger("SeedAttack");


        if (stickArmAnimator != null)
            stickArmAnimator.ResetTrigger("stickAttack");


        if (emptyArmAnimator != null)
            emptyArmAnimator.ResetTrigger("stickAttack");
    }


    void StopCurrentActions()
    {
        if (bossMovement != null)
            bossMovement.Stop();
    }
    void EnterDeadState()
    {
        if (CurrentState == BossState.Dead)
            return;

        CurrentState = BossState.Dead;

        StopCurrentActions();

        ResetAttackTriggers();

        if (bodyAnimator != null)
            bodyAnimator.SetTrigger(deathTrigger);
        OnBossDied?.Invoke();

        StartCoroutine(DeathRoutine());
    }
    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathToCutsceneDelay);

        SceneManager.LoadScene(finalCutsceneScene);
    }
    public void StartSeedAttack()
    {
        if (CurrentState != BossState.Attacking)
            return;


        if (seedAttack == null)
            return;


        seedAttack.StartAttack();
    }

    public void StartStickAttack()
    {
        if (CurrentState != BossState.Attacking)
            return;


        if (stickAttack == null)
            return;


        stickAttack.StartAttack();
    }
}