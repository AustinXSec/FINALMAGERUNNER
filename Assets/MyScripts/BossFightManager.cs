using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;
using System.Collections;

public class BossFightManager : MonoBehaviour
{
    [Header("Boss References")]
    public Snake snake;                        // Assign Snake from scene
    public HellhoundSpawner hellhoundSpawner;   // Assign HellhoundSpawner from scene
    private Hellhound hellhoundInstance;       // Track spawned Hellhound

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

    void Start()
    {
        if (snake != null)
            snake.OnSnakeWakeUp += OnSnakeWokeUp;

        if (hellhoundSpawner != null)
            hellhoundSpawner.OnHellhoundSpawned += OnHellhoundSpawned;
    }

    void OnSnakeWokeUp()
    {
        // Optional: additional logic when Snake wakes
    }

    void OnHellhoundSpawned(Hellhound h)
    {
        hellhoundInstance = h;
    }

    void Update()
    {
        if (sceneLoading) return;

        bool snakeDead = snake == null || snake.IsDead;
        bool hellhoundDead = hellhoundInstance == null || hellhoundInstance.IsDead;

        if (snakeDead && hellhoundDead)
        {
            // Fade out boss music
            if (MusicManager.Instance != null)
                MusicManager.Instance.FadeOut(musicFadeSeconds);

            // Disable all enemy spawners and destroy all spawned enemies only once
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
            // Destroy all currently spawned enemies
            foreach (GameObject enemy in spawner.spawnedEnemies)
            {
                if (enemy != null)
                    Destroy(enemy);
            }
            spawner.spawnedEnemies.Clear();

            // Stop spawning loop and hide the spawner
            spawner.StopAllCoroutines();
            spawner.enabled = false;
            spawner.gameObject.SetActive(false);
        }

        Debug.Log($"Disabled {spawners.Length} enemy spawners and destroyed all spawned enemies.");
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
