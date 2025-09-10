using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // <-- this is needed for IEnumerator

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "BossStage"; // scene to load

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object entering the trigger is the player
        if (collision.CompareTag("Player"))
        {
            // Optionally, add a delay before changing scene
            StartCoroutine(LoadNextSceneAfterDelay(4f)); // 1 second delay
        }
    }

    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextSceneName);
    }
}
