using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelDoor : MonoBehaviour
{
    [Header("Next Level Settings")]
    public string nextLevelName; // Name of the scene to load

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Only the player triggers it
        {
            // Optional: Play door animation or fade out before loading
            SceneManager.LoadScene(nextLevelName);
        }
    }
}
