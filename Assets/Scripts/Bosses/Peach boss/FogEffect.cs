using System.Collections;
using UnityEngine;

public class FogEffect : MonoBehaviour
{
    public static FogEffect Instance { get; private set; }

    [SerializeField] GameObject fogRoot;
    [SerializeField] float duration = 1.5f;

    Coroutine activeRoutine;

    void Awake()
    {
        Instance = this;

        if (fogRoot != null)
            fogRoot.SetActive(false);
    }

    public void Trigger()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(FogRoutine());
    }

    IEnumerator FogRoutine()
    {
        if (fogRoot != null)
            fogRoot.SetActive(true);

        yield return new WaitForSeconds(duration);

        if (fogRoot != null)
            fogRoot.SetActive(false);

        activeRoutine = null;
    }
}