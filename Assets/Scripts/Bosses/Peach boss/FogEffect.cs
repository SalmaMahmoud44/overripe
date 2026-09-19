using System.Collections;
using UnityEngine;

public class FogEffect : MonoBehaviour
{
    public static FogEffect Instance { get; private set; }

    [SerializeField] GameObject fogRoot;
    [SerializeField] float duration = 1.5f;

    Coroutine activeRoutine;
    AudioSource fogAudio;

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

        if (fogRoot != null)
            fogRoot.SetActive(true);

        if (AudioManager.Instance != null)
        {
            fogAudio = AudioManager.Instance.PlayLoopSFX(
                AudioManager.Instance.FogEffectClip
            );
        }

        activeRoutine = StartCoroutine(FogRoutine());
    }

    IEnumerator FogRoutine()
    {
        yield return new WaitForSeconds(duration);

        if (fogRoot != null)
            fogRoot.SetActive(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLoopSFX(fogAudio);
        }

        fogAudio = null;
        activeRoutine = null;
    }
}