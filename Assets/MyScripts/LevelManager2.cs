using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition; // namespace from your asset

public class LevelManager2 : MonoBehaviour
{
    [Header("Next Scene Name")]
    public string nextSceneName; // e.g., Level 3

    [Header("Optional Transition")]
    public TransitionSettings myTransition; // assign in inspector

    private bool sceneLoading = false; // prevent multiple loads

    void Update()
    {
        if (sceneLoading) return;

        // Find all enemies currently in the scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // If no enemies exist, load the next scene
        if (enemies.Length == 0)
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        sceneLoading = true; // prevent multiple calls

        // Try to get TransitionManager instance
        TransitionManager manager = null;
        try
        {
            manager = TransitionManager.Instance();
        }
        catch
        {
            // Ignore if not ready
        }

        if (manager != null && myTransition != null)
        {
            // Use transition to load next scene
            manager.Transition(nextSceneName, myTransition, 0f);
        }
        else
        {
            // Fallback to instant load
            if (manager == null)
                Debug.LogWarning("TransitionManager not found. Loading scene instantly.");
            else
                Debug.LogWarning("TransitionSettings not assigned. Loading scene instantly.");

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
