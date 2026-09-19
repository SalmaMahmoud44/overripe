using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class AppleBossMovement : MonoBehaviour
{
    [Header("Movement Points")]
    [SerializeField] Transform leftPoint;
    [SerializeField] Transform centerPoint;
    [SerializeField] Transform rightPoint;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float stopDistance = 0.05f;

    [Header("Boss Movement Camera Shake")]
    [SerializeField] private CinemachineImpulseSource movementImpulse;
    [SerializeField] private float shakeInterval = 0.35f;

    public bool IsMoving { get; private set; }

    public Transform CurrentPoint { get; private set; }

    private Coroutine moveRoutine;
    private Coroutine shakeRoutine;

    private void Start()
    {
        CurrentPoint = centerPoint;
    }

    public void MoveToLeft()
    {
        MoveTo(leftPoint);
    }

    public void MoveToRight()
    {
        MoveTo(rightPoint);
    }
    public void MoveToCenter()
    {
        MoveTo(centerPoint);
    }

    public void MoveTo(Transform targetPoint)
    {
        if (targetPoint == null) return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(targetPoint));
    }

    private IEnumerator MoveRoutine(Transform targetPoint)
    {
        IsMoving = true;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.appleWalkClip);

        shakeRoutine = StartCoroutine(MovementShakeRoutine());

        while (Mathf.Abs(transform.position.x - targetPoint.position.x) > stopDistance)
        {
            Vector3 targetPos = new Vector3( targetPoint.position.x, transform.position.y, transform.position.z );

            transform.position = Vector3.MoveTowards( transform.position, targetPos,moveSpeed * Time.deltaTime );

            yield return null;
        }

        transform.position = new Vector3(targetPoint.position.x,transform.position.y, transform.position.z);

        IsMoving = false;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        CurrentPoint = targetPoint;
        moveRoutine = null;
    }
    private IEnumerator MovementShakeRoutine()
    {
        while (IsMoving)
        {
            yield return new WaitForSeconds(shakeInterval);

            if (!IsMoving)
                yield break;

            if (movementImpulse != null)
            {
                movementImpulse.GenerateImpulse();
            }
        }
    }
    public void Stop()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
        IsMoving = false;
    }



}
