using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public void PlayGame()
    {
        // Gets the current active scene's index and adds 1 to load the next one
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // Checks if the next scene exists in the build settings before loading
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next scene found in Build Settings!");
        }
    }
}