using UnityEngine;
using UnityEngine.SceneManagement;

// Handles menu button callbacks: Play, Settings, Quit.
public class MenuButtonHandler : MonoBehaviour
{
    // Called when Play button is clicked
    public void OnPlayClicked()
    {
        Debug.Log("Play clicked");
        // TODO: Replace the Debug.Log with your scene load when ready:
        // SceneManager.LoadScene("GameScene");
    }

    // Called when Settings button is clicked
    public void OnSettingsClicked()
    {
        Debug.Log("Settings clicked");
        // Implement settings UI here in the future.
    }

    // Called when Quit button is clicked
    public void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
        Application.Quit();
    }
}
