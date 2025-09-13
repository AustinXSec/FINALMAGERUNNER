using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    // Source A and B used to crossfade between tracks
    public AudioSource sourceA;
    public AudioSource sourceB;

    // which source is currently active (playing the "main" music)
    private AudioSource activeSource;
    private AudioSource inactiveSource;

    [Header("Defaults")]
    [Tooltip("Ambient / corridor music that plays on scene start (optional)")]
    public AudioClip ambientClip;
    [Tooltip("Volume for music (0..1)")]
    [Range(0f, 1f)] public float musicVolume = 0.8f;

    private Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // if inspector sources not assigned, create them
        if (sourceA == null || sourceB == null)
        {
            // create two audio source children if missing
            if (sourceA == null)
            {
                GameObject go = new GameObject("MusicSourceA");
                go.transform.SetParent(transform);
                sourceA = go.AddComponent<AudioSource>();
                sourceA.playOnAwake = false;
            }
            if (sourceB == null)
            {
                GameObject go2 = new GameObject("MusicSourceB");
                go2.transform.SetParent(transform);
                sourceB = go2.AddComponent<AudioSource>();
                sourceB.playOnAwake = false;
            }
        }

        sourceA.loop = true;
        sourceB.loop = true;
        sourceA.volume = 0f;
        sourceB.volume = 0f;

        activeSource = sourceA;
        inactiveSource = sourceB;
    }

    void Start()
    {
        // Start ambient if assigned
        if (ambientClip != null)
        {
            PlayImmediate(ambientClip, true, musicVolume);
            activeSource.volume = musicVolume;
        }
    }

    /// <summary>
    /// Immediately play clip on the active source (stops fade routines).
    /// </summary>
    public void PlayImmediate(AudioClip clip, bool loop = true, float volume = 0.8f)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        activeSource.clip = clip;
        activeSource.loop = loop;
        activeSource.volume = volume;
        activeSource.Play();
        inactiveSource.Stop();
    }

    /// <summary>
    /// Crossfade to the provided clip over fadeSeconds. If loop = true, the new clip will loop.
    /// </summary>
    public void CrossfadeTo(AudioClip newClip, float fadeSeconds = 1.5f, bool loop = true)
    {
        if (newClip == null) return;

        // if the requested clip is already playing on the active source, just ensure loop/volume
        if (activeSource.clip == newClip && activeSource.isPlaying)
        {
            activeSource.loop = loop;
            activeSource.volume = musicVolume;
            return;
        }

        // prepare inactive source
        inactiveSource.clip = newClip;
        inactiveSource.loop = loop;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(CrossfadeCoroutine(fadeSeconds));
    }

    private IEnumerator CrossfadeCoroutine(float duration)
    {
        float t = 0f;
        float startVolActive = activeSource.volume;
        float startVolInactive = inactiveSource.volume;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / duration);
            activeSource.volume = Mathf.Lerp(startVolActive, 0f, alpha);
            inactiveSource.volume = Mathf.Lerp(startVolInactive, musicVolume, alpha);
            yield return null;
        }

        // finalize
        activeSource.volume = 0f;
        inactiveSource.volume = musicVolume;

        // swap
        AudioSource tmp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = tmp;

        // stop the now-inactive source (optional)
        inactiveSource.Stop();

        fadeRoutine = null;
        yield break;
    }

    /// <summary>
    /// Fade out current music to silence over seconds.
    /// </summary>
    public void FadeOut(float seconds)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutCoroutine(seconds));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float t = 0f;
        float startVol = activeSource.volume;
        while (t < duration)
        {
            t += Time.deltaTime;
            activeSource.volume = Mathf.Lerp(startVol, 0f, Mathf.Clamp01(t / duration));
            yield return null;
        }
        activeSource.Stop();
        fadeRoutine = null;
    }
}
