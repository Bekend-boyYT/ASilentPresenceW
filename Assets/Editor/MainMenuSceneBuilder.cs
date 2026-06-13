using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using TMPro;

// Editor utility to build a main menu scene layout automatically in a fresh scene.
public class MainMenuSceneBuilder
{
    [MenuItem("Tools/Build Horror Main Menu Scene")]
    public static void BuildMenuScene()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create EventSystem if missing
        if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }

        // Logo (TextMeshPro)
        GameObject logoGO = new GameObject("LogoText");
        logoGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI logoText = logoGO.AddComponent<TextMeshProUGUI>();
        logoText.text = "Made by Popstar Games";
        logoText.alignment = TextAlignmentOptions.Center;
        logoText.fontSize = 48;
        RectTransform logoRT = logoGO.GetComponent<RectTransform>();
        logoRT.anchorMin = new Vector2(0.5f, 0.5f);
        logoRT.anchorMax = new Vector2(0.5f, 0.5f);
        logoRT.anchoredPosition = Vector2.zero;
        logoRT.sizeDelta = new Vector2(800, 200);

        CanvasGroup logoCG = logoGO.AddComponent<CanvasGroup>();

        // Menu panel
        GameObject menuPanel = new GameObject("MenuPanel");
        menuPanel.transform.SetParent(canvasGO.transform, false);
        RectTransform menuRT = menuPanel.AddComponent<RectTransform>();
        menuRT.anchorMin = new Vector2(0.5f, 0.2f);
        menuRT.anchorMax = new Vector2(0.5f, 0.2f);
        menuRT.anchoredPosition = Vector2.zero;
        menuRT.sizeDelta = new Vector2(600, 300);

        CanvasGroup menuCG = menuPanel.AddComponent<CanvasGroup>();

        // Title
        GameObject titleGO = new GameObject("GameTitle");
        titleGO.transform.SetParent(menuPanel.transform, false);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "Your Game Title";
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 64;
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, -40);
        titleRT.sizeDelta = new Vector2(800, 120);

        // Buttons: Play, Settings, Quit
        CreateButton(menuPanel.transform, "PlayButton", "Play", new Vector2(0, 20));
        CreateButton(menuPanel.transform, "SettingsButton", "Settings", new Vector2(0, -60));
        CreateButton(menuPanel.transform, "QuitButton", "Quit", new Vector2(0, -140));

        // Create start and end camera transforms
        GameObject startT = new GameObject("CameraStart");
        startT.transform.position = Vector3.zero;
        startT.transform.rotation = Quaternion.LookRotation(Vector3.up);

        GameObject endT = new GameObject("CameraEnd");
        endT.transform.position = Vector3.zero;
        endT.transform.rotation = Quaternion.LookRotation(Vector3.forward);

        // Find main camera and attach controller
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        MainMenuController controller = cam.gameObject.GetComponent<MainMenuController>();
        if (controller == null)
            controller = cam.gameObject.AddComponent<MainMenuController>();

        // Assign references
        controller.startTransform = startT.transform;
        controller.endTransform = endT.transform;
        controller.logoCanvasGroup = logoCG;
        controller.menuCanvasGroup = menuCG;
        controller.mainCamera = cam;

        // Add AudioSource to camera for ambient audio
        AudioSource aSource = cam.gameObject.GetComponent<AudioSource>();
        if (aSource == null)
            aSource = cam.gameObject.AddComponent<AudioSource>();
        aSource.loop = true;
        controller.ambientAudioSource = aSource;

        // Add MenuButtonHandler and wire buttons
        MenuButtonHandler handler = menuPanel.AddComponent<MenuButtonHandler>();

        // Wire up the buttons by name
        Button[] buttons = menuPanel.GetComponentsInChildren<Button>();
        foreach (Button b in buttons)
        {
            if (b.name == "PlayButton")
                b.onClick.AddListener(handler.OnPlayClicked);
            else if (b.name == "SettingsButton")
                b.onClick.AddListener(handler.OnSettingsClicked);
            else if (b.name == "QuitButton")
                b.onClick.AddListener(handler.OnQuitClicked);
        }

        // Select objects in the editor and notify
        Selection.activeGameObject = canvasGO;
        Debug.Log("Horror Main Menu Scene built. Assign an ambient AudioClip to the camera AudioSource if desired.");
    }

    // Helper to create a simple button with TextMeshPro label
    private static void CreateButton(Transform parent, string name, string label, Vector2 anchoredPos)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);

        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 80);
        rt.anchoredPosition = anchoredPos;

        Button btn = btnGO.AddComponent<Button>();
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI t = txt.AddComponent<TextMeshProUGUI>();
        t.text = label;
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = 36;
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }
}
