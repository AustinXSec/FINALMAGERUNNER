using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition; // Namespace for your transition asset

public class LevelDoor : MonoBehaviour
{
    [Header("Next Level Settings")]
    public string nextLevelName; // Name of the scene to load

    [Header("Optional Transition")]
    public TransitionSettings myTransition; // Assign in inspector

    private bool isLoading = false; // Prevent multiple triggers

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isLoading) return; // Already loading
        if (!collision.CompareTag("Player")) return; // Only trigger for player

        isLoading = true;

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
            // Use transition to load the next level
            manager.Transition(nextLevelName, myTransition, 0f);
        }
        else
        {
            // Fallback to instant load
            if (manager == null)
                Debug.LogWarning("TransitionManager not found. Loading scene instantly.");
            else
                Debug.LogWarning("TransitionSettings not assigned. Loading scene instantly.");

            SceneManager.LoadScene(nextLevelName);
        }
    }
}
