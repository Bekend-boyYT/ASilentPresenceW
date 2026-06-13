using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif
using TMPro;

// Controls the intro sequence, camera animation, audio, and menu activation.
public class MainMenuController : MonoBehaviour
{
    [Header("Camera Transforms")]
    // Transform representing the starting orientation (looking up)
    public Transform startTransform;
    // Transform representing the ending orientation (looking forward)
    public Transform endTransform;

    [Header("Timing")]
    // How long the logo stays visible before the camera starts moving
    public float logoDuration = 3f;
    // How long the camera move takes (exactly)
    public float cameraMoveDuration = 3f;
    // How long the menu fade takes after the camera finishes
    public float menuFadeDuration = 1f;

    [Header("Audio")]
    // Ambient audio source that should loop and play on scene start
    public AudioSource ambientAudioSource;

    [Header("References")]
    // CanvasGroup for the logo (used to fade in/out)
    public CanvasGroup logoCanvasGroup;
    // CanvasGroup for the main menu (hidden initially)
    public CanvasGroup menuCanvasGroup;
    // Main camera reference (will default to Camera.main if empty)
    public Camera mainCamera;

    [Header("Easing")]
    // Optional easing curve for camera motion (default ease-in-out)
    public AnimationCurve cameraEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Internal state
    private Coroutine introCoroutine;
    private bool introPlaying = false;

    void Start()
    {
        // Ensure we have a camera reference
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Set initial camera rotation to startTransform if provided
        if (startTransform != null)
            mainCamera.transform.rotation = startTransform.rotation;
        else
            mainCamera.transform.rotation = Quaternion.LookRotation(Vector3.up);

        // Ensure menu is hidden and not interactable
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        // Start ambient audio if provided
        if (ambientAudioSource != null)
        {
            ambientAudioSource.loop = true;
            ambientAudioSource.playOnAwake = false;
            ambientAudioSource.Play();
        }

        // Kick off the intro sequence
        introCoroutine = StartCoroutine(IntroSequence());
    }

    void Update()
    {
        // Allow skipping the intro by pressing any key
        if (introPlaying && IsAnyKeyPressed())
        {
            SkipIntroImmediate();
        }
    }

    // Returns true if any key or button was pressed this frame.
    private bool IsAnyKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame)
                return true;
        }

        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                    return true;
            }
        }

        return false;
#else
        return Input.anyKeyDown;
#endif
    }

    // Main coroutine driving the intro sequence
    private IEnumerator IntroSequence()
    {
        introPlaying = true;

        // Fade logo in quickly
        if (logoCanvasGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(logoCanvasGroup, 0f, 1f, 0.5f));

        // Wait while logo remains visible
        float elapsed = 0f;
        while (elapsed < logoDuration)
        {
            if (IsAnyKeyPressed())
            {
                SkipIntroImmediate();
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Start camera motion and fade logo out concurrently
        float t = 0f;
        Quaternion startRot = startTransform != null ? startTransform.rotation : Quaternion.LookRotation(Vector3.up);
        Quaternion endRot = endTransform != null ? endTransform.rotation : Quaternion.LookRotation(Vector3.forward);

        while (t < cameraMoveDuration)
        {
            if (IsAnyKeyPressed())
            {
                SkipIntroImmediate();
                yield break;
            }

            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / cameraMoveDuration);
            float eased = cameraEasing.Evaluate(normalized);

            // Smoothly rotate the camera using Slerp with easing
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, eased);

            // Fade the logo out over the camera movement
            if (logoCanvasGroup != null)
                logoCanvasGroup.alpha = Mathf.Lerp(1f, 0f, eased);

            yield return null;
        }

        // Ensure final values are exact
        mainCamera.transform.rotation = endRot;
        if (logoCanvasGroup != null)
            logoCanvasGroup.alpha = 0f;

        // Fade menu in and enable interaction
        if (menuCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(menuCanvasGroup, 0f, 1f, menuFadeDuration));
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }

        introPlaying = false;
    }

    // Immediately skip the intro sequence and show the menu
    public void SkipIntroImmediate()
    {
        // Stop the intro coroutine if running
        if (introCoroutine != null)
            StopCoroutine(introCoroutine);

        introPlaying = false;

        // Jump camera to final rotation
        if (endTransform != null && mainCamera != null)
            mainCamera.transform.rotation = endTransform.rotation;

        // Hide the logo
        if (logoCanvasGroup != null)
            logoCanvasGroup.alpha = 0f;

        // Show the menu immediately and make it interactive
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }
    }

    // Utility coroutine to fade a CanvasGroup's alpha from 'from' to 'to' over 'duration' seconds
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null)
            yield break;

        float elapsed = 0f;
        cg.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }
}
