using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using EasyTransition;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI textComponent;   // assign your TMP text object
    public CanvasGroup canvasGroup;         // assign same object's CanvasGroup

    [TextArea(3,8)]
    public string[] lines = new string[]
    {
        "Wake now, hero...",
        "Your trial begins.",
        "The forest awaits."
    };

    [Header("Timing Settings")]
    public float fadeDuration = 1.0f;
    public float displayDuration = 2.0f;

    [Header("Scene Settings")]
    public string nextSceneName = "Level_1"; // exact scene name of Level1

    [Header("Transition Settings (Optional)")]
    public TransitionSettings myTransition; // assign your EasyTransition settings

    [Header("Heartbeat Audio (Optional)")]
    public AudioSource heartbeatSource;     
    [Range(0f,1f)] public float heartbeatStartVolume = 0.12f;
    [Range(0f,1f)] public float heartbeatTargetVolume = 0.65f;
    [Range(0f,2f)] public float heartbeatStartPitch = 1.0f;
    [Range(0f,2f)] public float heartbeatTargetPitch = 1.12f;
    public float heartbeatRampTime = 1.2f;  

    void Start()
    {
        if (textComponent == null || canvasGroup == null)
        {
            Debug.LogError("CutsceneManager: assign TextMeshProUGUI and CanvasGroup in inspector.");
            return;
        }

        // Setup heartbeat if assigned
        if (heartbeatSource != null)
        {
            heartbeatSource.loop = true;
            heartbeatSource.volume = heartbeatStartVolume;
            heartbeatSource.pitch = heartbeatStartPitch;
            heartbeatSource.Play();
        }

        canvasGroup.alpha = 0f; // hide text initially
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        foreach (string line in lines)
        {
            textComponent.text = line;

            // fade in
            yield return StartCoroutine(FadeCanvasAlpha(0f, 1f));

            // display duration
            yield return new WaitForSeconds(displayDuration);

            // fade out
            yield return StartCoroutine(FadeCanvasAlpha(1f, 0f));
        }

        // ramp heartbeat if assigned
        if (heartbeatSource != null && heartbeatRampTime > 0f)
        {
            float t = 0f;
            float startVol = heartbeatSource.volume;
            float startPitch = heartbeatSource.pitch;
            while (t < heartbeatRampTime)
            {
                t += Time.deltaTime;
                heartbeatSource.volume = Mathf.Lerp(startVol, heartbeatTargetVolume, t / heartbeatRampTime);
                heartbeatSource.pitch = Mathf.Lerp(startPitch, heartbeatTargetPitch, t / heartbeatRampTime);
                yield return null;
            }
        }

        if (heartbeatSource != null) heartbeatSource.Stop();

        // wait 3 seconds before transition
        yield return new WaitForSeconds(0f);

        // Load next scene using EasyTransition if assigned
        TransitionManager manager = null;
        try
        {
            manager = TransitionManager.Instance();
        }
        catch { }

        if (manager != null && myTransition != null)
        {
            manager.Transition(nextSceneName, myTransition, 0f);
        }
        else
        {
            if (manager == null)
                Debug.LogWarning("TransitionManager not found. Loading scene instantly.");
            else
                Debug.LogWarning("TransitionSettings not assigned. Loading scene instantly.");

            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator FadeCanvasAlpha(float start, float end)
    {
        float elapsed = 0f;
        float dur = Mathf.Max(0.01f, fadeDuration);
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / dur);
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}
