using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using EasyTransition; // Namespace for your transition asset

public class DoorTrigger : MonoBehaviour
{
    [Header("Next Scene Settings")]
    [SerializeField] private string nextSceneName = "BossStage"; // scene to load

    [Header("Optional Transition")]
    public TransitionSettings myTransition; // assign in inspector

    [Header("Optional Delay Before Transition")]
    public float delayBeforeTransition = 1f; // seconds

    private bool isLoading = false; // prevent multiple triggers

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isLoading) return;
        if (!collision.CompareTag("Player")) return;

        isLoading = true;
        StartCoroutine(LoadNextSceneAfterDelay(delayBeforeTransition));
    }

    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        // Wait for optional delay
        yield return new WaitForSeconds(delay);

        // Try to get the TransitionManager instance
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
