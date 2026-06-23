using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for modern TextMeshPro Dropdowns and Text

public class SettingsMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject confirmationPanel;

    [Header("UI Elements")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public TMP_Text timerText;

    private List<Resolution> uniqueResolutions = new List<Resolution>();
    private int currentResolutionIndex = 0;

    // Variables to store previous stable settings for reverting
    private int previousResolutionIndex;
    private bool previousFullscreen;

    private Coroutine revertCoroutine;
    private const float countdownTime = 30f;

    void Start()
    {
        // Ensure panels start closed
        settingsPanel.SetActive(false);
        confirmationPanel.SetActive(false);

        FilterAndPopulateResolutions();
        LoadSettings();
    }

    // Filters out duplicate resolutions (caused by different refresh rates) 
    // and populates the dropdown cleanly.
    void FilterAndPopulateResolutions()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            Resolution res = Screen.resolutions[i];
            bool isDuplicate = false;

            foreach (var uniqueRes in uniqueResolutions)
            {
                if (uniqueRes.width == res.width && uniqueRes.height == res.height)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                uniqueResolutions.Add(res);
                string option = res.width + " x " + res.height;
                options.Add(option);
            }
        }

        resolutionDropdown.AddOptions(options);
    }

    void LoadSettings()
    {
        // Load saved settings, or fall back to defaults if first time running game
        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (savedResIndex != -1 && savedResIndex < uniqueResolutions.Count)
        {
            currentResolutionIndex = savedResIndex;
        }
        else
        {
            // Find screen default resolution index
            for (int i = 0; i < uniqueResolutions.Count; i++)
            {
                if (uniqueResolutions[i].width == Screen.currentResolution.width &&
                    uniqueResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                    break;
                }
            }
        }

        // Apply loaded settings to screen and UI
        Screen.SetResolution(uniqueResolutions[currentResolutionIndex].width, uniqueResolutions[currentResolutionIndex].height, savedFullscreen);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        fullscreenToggle.isOn = savedFullscreen;
    }

    // --- Open / Close Settings Menu ---
    public void OpenSettings() => settingsPanel.SetActive(true);
    public void CloseSettings() => settingsPanel.SetActive(false);

    // --- Apply Settings & Countdown Logic ---
    public void ApplySettings()
    {
        // 1. Back up current working settings before trying new ones
        previousResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        previousFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1; // Fixed: now properly converts int to bool

        // 2. Apply new temporary choices
        int targetIndex = resolutionDropdown.value;
        bool targetFullscreen = fullscreenToggle.isOn;
        Screen.SetResolution(uniqueResolutions[targetIndex].width, uniqueResolutions[targetIndex].height, targetFullscreen);

        // 3. Launch confirmation dialog and start the 30-second timer
        confirmationPanel.SetActive(true);
        if (revertCoroutine != null) StopCoroutine(revertCoroutine);
        revertCoroutine = StartCoroutine(RevertTimerRoutine());
    }

    private IEnumerator RevertTimerRoutine()
    {
        float timeLeft = countdownTime;
        while (timeLeft > 0)
        {
            timerText.text = $"Keep changes? Reverting in {Mathf.CeilToInt(timeLeft)}s";
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        // Timer finished without user input -> Auto Revert
        RevertChanges();
    }

    public void KeepChanges()
    {
        if (revertCoroutine != null) StopCoroutine(revertCoroutine);
        confirmationPanel.SetActive(false);

        // Permanently save settings to PlayerPrefs
        currentResolutionIndex = resolutionDropdown.value;
        PlayerPrefs.SetInt("ResolutionIndex", currentResolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void RevertChanges()
    {
        if (revertCoroutine != null) StopCoroutine(revertCoroutine);
        confirmationPanel.SetActive(false);

        // Reapply previous working resolution
        Screen.SetResolution(uniqueResolutions[previousResolutionIndex].width, uniqueResolutions[previousResolutionIndex].height, previousFullscreen);

        // Reset UI items back to old settings
        resolutionDropdown.value = previousResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        fullscreenToggle.isOn = previousFullscreen;
    }
}