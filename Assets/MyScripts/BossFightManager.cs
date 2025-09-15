using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;
using System.Collections;

public class BossFightManager : MonoBehaviour
{
    [Header("Next Scene Name")]
    public string nextSceneName;

    [Header("Optional Transition")]
    public TransitionSettings myTransition;

    [Header("Music Fade Out Duration")]
    public float musicFadeSeconds = 1.5f;

    [Header("Delay Before Scene Load (seconds)")]
    public float delayBeforeLoad = 10f;

    private bool sceneLoading = false;
    private bool spawnersDisabled = false;

    void Update()
    {
        if (sceneLoading) return;

        Snake snake = FindObjectOfType<Snake>();
        Hellhound hellhound = FindObjectOfType<Hellhound>();

        bool snakeDead = (snake == null || snake.IsDead);
        bool hellhoundDead = (hellhound == null || hellhound.IsDead);

        if (snakeDead && hellhoundDead)
        {
            // Fade out boss music
            if (MusicManager.Instance != null)
                MusicManager.Instance.FadeOut(musicFadeSeconds);

            // Disable all enemy spawners only once
            if (!spawnersDisabled)
            {
                DisableAllSpawners();
                spawnersDisabled = true;
            }

            // Start delayed scene load
            if (!sceneLoading)
            {
                sceneLoading = true;
                StartCoroutine(DelayedSceneLoad());
            }
        }
    }

    private void DisableAllSpawners()
    {
        Enemy2pawner[] spawners = FindObjectsOfType<Enemy2pawner>();
        foreach (Enemy2pawner spawner in spawners)
        {
            spawner.enabled = false;        // stop Update
            spawner.StopAllCoroutines();    // stop spawning loop
            spawner.gameObject.SetActive(false); // hide spawner if you want
        }

        Debug.Log($"Disabled {spawners.Length} enemy spawners.");
    }

    private IEnumerator DelayedSceneLoad()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        LoadNextScene();
    }

    private void LoadNextScene()
    {
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
}
