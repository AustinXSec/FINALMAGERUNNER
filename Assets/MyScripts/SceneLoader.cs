using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Loads a scene by name.
    /// </summary>
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
