using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quit button pressed!");

        // Closes the actual built application
        Application.Quit();

        // Stops play mode inside the Unity Editor (for testing)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}