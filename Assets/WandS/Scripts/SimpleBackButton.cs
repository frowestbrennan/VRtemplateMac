using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SimpleBackButton : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip backButtonSound;

    [Header("Controller Hiding")]
    [SerializeField] private bool hideControllersOnStart = true;

    // Input Actions
    private InputAction leftBButton;
    private InputAction rightBButton;
    private InputActionMap controllerMap;

    void Start()
    {
        SetupAudio();
        SetupInput();

        if (hideControllersOnStart)
        {
            HideAllControllers();
        }
    }

    void SetupAudio()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void SetupInput()
    {
        // Create input action map
        controllerMap = new InputActionMap("BackButton");

        // Left controller B button
        leftBButton = controllerMap.AddAction("LeftB", InputActionType.Button);
        leftBButton.AddBinding("<XRController>{LeftHand}/secondaryButton");
        leftBButton.performed += OnBackButtonPressed;

        // Right controller B button  
        rightBButton = controllerMap.AddAction("RightB", InputActionType.Button);
        rightBButton.AddBinding("<XRController>{RightHand}/secondaryButton");
        rightBButton.performed += OnBackButtonPressed;

        // Also add keyboard support for testing
        var keyboardB = controllerMap.AddAction("KeyboardB", InputActionType.Button);
        keyboardB.AddBinding("<Keyboard>/b");
        keyboardB.performed += OnBackButtonPressed;

        // Enable the action map
        controllerMap.Enable();

        Debug.Log("Back button input setup complete");
    }

    void HideAllControllers()
    {
        // Find common controller object names
        string[] controllerNames = {
            "LeftHand Controller",
            "RightHand Controller",
            "Left Controller",
            "Right Controller",
            "LeftHandController",
            "RightHandController",
            "XR Origin/Camera Offset/LeftHand Controller",
            "XR Origin/Camera Offset/RightHand Controller"
        };

        foreach (string controllerName in controllerNames)
        {
            GameObject controller = GameObject.Find(controllerName);
            if (controller != null)
            {
                HideControllerVisuals(controller);
            }
        }

        // Also search by component type
        var allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (var renderer in allRenderers)
        {
            if (IsControllerRenderer(renderer))
            {
                renderer.enabled = false;
                Debug.Log($"Hid controller renderer: {renderer.gameObject.name}");
            }
        }

        // Hide line renderers (controller rays)
        var lineRenderers = FindObjectsByType<LineRenderer>(FindObjectsSortMode.None);
        foreach (var lineRenderer in lineRenderers)
        {
            if (IsControllerLineRenderer(lineRenderer))
            {
                lineRenderer.enabled = false;
                Debug.Log($"Hid controller line: {lineRenderer.gameObject.name}");
            }
        }
    }

    void HideControllerVisuals(GameObject controller)
    {
        // Hide all child renderers
        Renderer[] renderers = controller.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Hide line renderers  
        LineRenderer[] lines = controller.GetComponentsInChildren<LineRenderer>();
        foreach (var line in lines)
        {
            line.enabled = false;
        }

        Debug.Log($"Hid visuals for: {controller.name}");
    }

    bool IsControllerRenderer(Renderer renderer)
    {
        string objName = renderer.gameObject.name.ToLower();
        return objName.Contains("controller") ||
               objName.Contains("hand") ||
               objName.Contains("oculus") ||
               objName.Contains("quest") ||
               renderer.gameObject.transform.parent?.name.ToLower().Contains("controller") == true;
    }

    bool IsControllerLineRenderer(LineRenderer lineRenderer)
    {
        string objName = lineRenderer.gameObject.name.ToLower();
        return objName.Contains("ray") ||
               objName.Contains("line") ||
               objName.Contains("interactor") ||
               lineRenderer.gameObject.transform.parent?.name.ToLower().Contains("controller") == true;
    }

    void OnBackButtonPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Back button pressed!");
        GoBackToMainMenu();
    }

    void GoBackToMainMenu()
    {
        PlaySound(backButtonSound);

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.Log($"Loading scene: {mainMenuSceneName}");
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("Main menu scene name not set!");
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Public methods
    public void SetMainMenuScene(string sceneName)
    {
        mainMenuSceneName = sceneName;
    }

    // Manual trigger for testing
    public void TriggerBackButton()
    {
        GoBackToMainMenu();
    }

    void OnDestroy()
    {
        // Clean up
        if (controllerMap != null)
        {
            controllerMap.Disable();
            controllerMap.Dispose();
        }
    }
}