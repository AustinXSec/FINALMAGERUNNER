using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerControls : MonoBehaviour
{
    [Header("Player Keys")]
    public KeyCode quitKey = KeyCode.Escape;   // Esc = quit game
    public KeyCode restartKey = KeyCode.R;     // R = restart current level

    [Header("Restart Settings")]
    public float restartDelay = 5f; // Delay in seconds
    private bool isRestarting = false;

    void Update()
    {
        // Quit the game
        if (Input.GetKeyDown(quitKey))
        {
            QuitGame();
        }

        // Restart the current scene with delay
        if (Input.GetKeyDown(restartKey) && !isRestarting)
        {
            StartCoroutine(RestartSceneWithDelay());
        }
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game pressed"); // Shows in editor
    }

    IEnumerator RestartSceneWithDelay()
    {
        isRestarting = true;
        Debug.Log("Restarting in " + restartDelay + " seconds...");

        yield return new WaitForSeconds(restartDelay);

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
