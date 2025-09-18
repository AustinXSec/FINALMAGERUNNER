using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;
using TMPro;

public class LevelManager2 : MonoBehaviour
{
    [Header("Next Scene Name")]
    public string nextSceneName; // e.g., Level 3

    [Header("Optional Transition")]
    public TransitionSettings myTransition; // assign in inspector

    [Header("UI")]
    public TextMeshProUGUI objectiveText; // assign in inspector

    private bool sceneLoading = false; 
    private int totalEnemies;

    void Start()
    {
        // Count total enemies at the start
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        UpdateObjectiveText(totalEnemies);
    }

    void Update()
    {
        if (sceneLoading) return;

        // Count how many enemies remain
        int remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;

        UpdateObjectiveText(remainingEnemies);

        // If no enemies remain, load next scene
        if (remainingEnemies == 0)
        {
            LoadNextScene();
        }
    }

    private void UpdateObjectiveText(int remaining)
    {
        if (objectiveText != null)
        {
            objectiveText.text =
                "Defeat all enemies\n" + remaining + " / " + totalEnemies;
        }
    }

    private void LoadNextScene()
    {
        sceneLoading = true; 

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
