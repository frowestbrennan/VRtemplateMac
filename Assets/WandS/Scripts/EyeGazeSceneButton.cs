using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class EyeGazeSceneButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "Scene1";

    [Header("Animation Settings")]
    [SerializeField] private float fillDuration = 2.0f;
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Button Animation")]
    [SerializeField] private float hoverAnimationDuration = 0.3f;
    [SerializeField] private float hoverUpDistance = 0.1f; // Distance to move up in world units
    [SerializeField] private AnimationCurve hoverCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float hoverScale = 1.05f; // Scale multiplier when hovered

    [Header("Visual Components")]
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private Image fillOverlay;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip selectSound;

    [Header("Events")]
    public UnityEvent OnGazeStart;
    public UnityEvent OnGazeEnd;
    public UnityEvent OnSceneTransition;

    private Coroutine hoverAnimationCoroutine;
    private bool isGazing = false;
    private bool isTransitioning = false;

    // Eye tracking variables
    private Camera mainCamera;
    private float gazeTimer = 0f;

    // Animation variables
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        InitializeComponents();
        SetupGazeInteraction();

        // Store original transform values
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
    }

    void InitializeComponents()
    {
        // Get main camera (usually the XR camera)
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }

        // Initialize fill overlay
        if (fillOverlay != null)
        {
            fillOverlay.fillMethod = Image.FillMethod.Vertical;
            fillOverlay.fillAmount = 0f;
            fillOverlay.color = new Color(1f, 1f, 1f, 0.7f); // Semi-transparent white
        }

        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void SetupGazeInteraction()
    {
        // Add collider if not present for raycasting
        if (GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;

            // Auto-size collider to image bounds if possible
            if (thumbnailImage != null)
            {
                RectTransform rectTransform = thumbnailImage.rectTransform;
                boxCollider.size = new Vector3(rectTransform.rect.width * 0.01f, rectTransform.rect.height * 0.01f, 1f);
            }
            else
            {
                boxCollider.size = Vector3.one;
            }
        }

        // Also setup Unity Button component if present for easier testing
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => TriggerSceneTransition());
        }

        Debug.Log($"EyeGazeButton Setup Complete for {gameObject.name}. Camera found: {mainCamera != null}");
    }

    void Update()
    {
        if (isTransitioning || mainCamera == null) return;

        CheckGazeInteraction();
    }

    void CheckGazeInteraction()
    {
        // Perform raycast from camera center (simulating eye gaze)
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        bool isHitting = Physics.Raycast(ray, out hit, 100f);
        bool isGazingAtThis = isHitting && hit.collider.gameObject == gameObject;

        // Debug logging
        if (isHitting)
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, isGazingAtThis ? Color.green : Color.yellow);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);
        }

        // Handle gaze enter
        if (isGazingAtThis && !isGazing)
        {
            Debug.Log($"Gaze entered: {gameObject.name}");
            OnGazeEnter();
        }
        // Handle gaze exit
        else if (!isGazingAtThis && isGazing)
        {
            Debug.Log($"Gaze exited: {gameObject.name}");
            OnGazeExit();
        }

        // Update gaze timer
        if (isGazing)
        {
            gazeTimer += Time.deltaTime;
            UpdateFillAmount();

            // Check if gaze duration completed
            if (gazeTimer >= fillDuration)
            {
                TriggerSceneTransition();
            }
        }
    }

    void OnGazeEnter()
    {
        if (isTransitioning) return;

        isGazing = true;
        gazeTimer = 0f;

        PlaySound(hoverSound);
        OnGazeStart?.Invoke();

        // Start hover animation
        StartHoverAnimation(true);
    }

    void OnGazeExit()
    {
        if (isTransitioning) return;

        isGazing = false;
        gazeTimer = 0f;

        // Reset fill overlay
        if (fillOverlay != null)
        {
            fillOverlay.fillAmount = 0f;
        }

        OnGazeEnd?.Invoke();

        // Start unhover animation
        StartHoverAnimation(false);
    }

    void UpdateFillAmount()
    {
        if (fillOverlay == null) return;

        float progress = gazeTimer / fillDuration;
        float curveValue = fillCurve.Evaluate(progress);
        fillOverlay.fillAmount = curveValue;
    }

    void TriggerSceneTransition()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        isGazing = false;

        PlaySound(selectSound);
        OnSceneTransition?.Invoke();

        // Load the scene immediately
        LoadTargetScene();
    }

    void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("Target scene name is empty! Please set the scene name in the inspector.");
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Public method to set scene name via code
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }

    // Public method to manually trigger the button
    public void ManualTrigger()
    {
        TriggerSceneTransition();
    }

    // Method to be called by XR Interaction Toolkit events if needed
    public void OnHoverEntered()
    {
        OnGazeEnter();
    }

    public void OnHoverExited()
    {
        OnGazeExit();
    }

    public void OnSelectEntered()
    {
        TriggerSceneTransition();
    }

    void StartHoverAnimation(bool hovering)
    {
        if (hoverAnimationCoroutine != null)
        {
            StopCoroutine(hoverAnimationCoroutine);
        }

        hoverAnimationCoroutine = StartCoroutine(HoverAnimationCoroutine(hovering));
    }

    IEnumerator HoverAnimationCoroutine(bool hovering)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.localPosition;
        Vector3 startScale = transform.localScale;

        Vector3 targetPosition = hovering ?
            originalPosition + Vector3.up * hoverUpDistance :
            originalPosition;

        Vector3 targetScale = hovering ?
            originalScale * hoverScale :
            originalScale;

        while (elapsedTime < hoverAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / hoverAnimationDuration;
            float curveValue = hoverCurve.Evaluate(progress);

            // Animate position
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);

            // Animate scale
            transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);

            yield return null;
        }

        // Ensure final values are set
        transform.localPosition = targetPosition;
        transform.localScale = targetScale;
        isHovered = hovering;
    }

    void OnDestroy()
    {
        // Clean up coroutines
        if (hoverAnimationCoroutine != null)
        {
            StopCoroutine(hoverAnimationCoroutine);
        }
    }

    // Optional: Draw gaze ray in scene view for debugging
    void OnDrawGizmos()
    {
        if (mainCamera != null)
        {
            Gizmos.color = isGazing ? Color.green : Color.red;
            Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * 10f);
        }
    }
}