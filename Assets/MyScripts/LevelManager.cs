using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject[] levelPrefabs; // Assign your level prefabs in the inspector
    private GameObject currentLevel;

    private int currentLevelIndex = 0;

    public Transform levelParent; // Optional: empty object to hold level prefab

    void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        if (currentLevel != null)
            Destroy(currentLevel);

        currentLevelIndex++;
        if (currentLevelIndex < levelPrefabs.Length)
        {
            LoadLevel(currentLevelIndex);
        }
        else
        {
            Debug.Log("No more levels!");
        }
    }

    private void LoadLevel(int index)
    {
        currentLevel = Instantiate(levelPrefabs[index], Vector3.zero, Quaternion.identity, levelParent);
    }
}
