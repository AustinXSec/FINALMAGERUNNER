using UnityEngine;
using EasyTransition;  

public class SceneLoader : MonoBehaviour
{
    public TransitionSettings myTransition; // assign in inspector

    public void LoadLevel(string levelName)
    {
        if (myTransition == null)
        {
            Debug.LogError("No transition assigned!");
            return;
        }

        TransitionManager.Instance().Transition(levelName, myTransition, 0f);
    }
}
