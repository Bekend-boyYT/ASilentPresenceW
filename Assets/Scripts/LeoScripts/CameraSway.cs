using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    [Header("Cursor Tracking")]
    [Tooltip("Maximum degrees the camera will tilt left or right.")]
    public float maxTiltY = 5f;
    
    [Tooltip("Maximum degrees the camera will tilt up or down.")]
    public float maxTiltX = 3f;
    
    [Tooltip("How smoothly the camera catches up to the cursor. Higher = faster.")]
    public float smoothSpeed = 3f;

    private Quaternion startRotation;

    void Start()
    {
        // FORCE MOUSE TO BE VISIBLE AND UNLOCKED FOR MENU BUTTONS
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Force the X rotation to 20 degrees, but keep your existing Y and Z angles from the editor
        Vector3 currentAngles = transform.localRotation.eulerAngles;
        startRotation = Quaternion.Euler(20f, currentAngles.y, currentAngles.z);
        
        // Apply the 20-degree X angle immediately when the game starts
        transform.localRotation = startRotation;
    }

    void Update()
    {
        // 1. GET NORMALIZED CURSOR POSITION (-1 to 1 range)
        // Center of the screen becomes (0,0)
        float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

        // Clamp values just in case the mouse goes outside the game window
        mouseX = Mathf.Clamp(mouseX, -1f, 1f);
        mouseY = Mathf.Clamp(mouseY, -1f, 1f);

        // 2. CALCULATE TARGET ROTATION
        // Mouse up (positive Y) tilts camera up (negative X rotation)
        // Mouse right (positive X) tilts camera right (positive Y rotation)
        float targetTiltX = -mouseY * maxTiltX;
        float targetTiltY = mouseX * maxTiltY;

        Quaternion targetRotation = startRotation * Quaternion.Euler(targetTiltX, targetTiltY, 0f);

        // 3. SMOOTHLY INTERPOLATE
        // Prevents jerky camera movements if the player moves the mouse too fast
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}