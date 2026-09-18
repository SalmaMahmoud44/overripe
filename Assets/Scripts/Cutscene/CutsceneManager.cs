using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutscenePanel
    {
        public CanvasGroup panelGroup;      // the full panel image
        public CanvasGroup[] textGroups;    // texts in this panel, in the order they should appear
    }

    [Header("Panels in order")]
    [SerializeField] private CutscenePanel[] panels;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Scene to load after the last panel")]
    [SerializeField] private string nextSceneName = "Banana";

    private void Start()
    {
        foreach (var p in panels)
        {
            p.panelGroup.alpha = 0f;
            p.panelGroup.gameObject.SetActive(false);

            foreach (var text in p.textGroups)
                text.alpha = 0f;
        }

        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        foreach (var panel in panels)
        {
            panel.panelGroup.gameObject.SetActive(true);
            yield return Fade(panel.panelGroup, 0f, 1f);

            foreach (var text in panel.textGroups)
            {
                yield return WaitForInput();
                yield return Fade(text, 0f, 1f);
            }

            yield return WaitForInput();
            yield return Fade(panel.panelGroup, 1f, 0f);
            panel.panelGroup.gameObject.SetActive(false);

            foreach (var text in panel.textGroups)
                text.alpha = 0f;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator WaitForInput()
    {
        yield return new WaitUntil(() =>
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame));
    }

    IEnumerator Fade(CanvasGroup group, float from, float to)
    {
        float t = 0f;
        group.alpha = from;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        group.alpha = to;
    }
}