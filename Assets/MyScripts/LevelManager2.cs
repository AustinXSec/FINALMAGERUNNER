using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager2 : MonoBehaviour
{
    [Header("Next Scene Name")]
    public string nextSceneName; // Set this to your Level 3 scene

    void Update()
    {
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
        // Optional: disable this script to prevent multiple calls
        enabled = false;

        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}
