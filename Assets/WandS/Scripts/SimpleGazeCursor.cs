using UnityEngine;

public class SimpleGazeCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private float cursorSize = 0.2f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private float cursorDistance = 0.3f;
    [SerializeField] private float maxDistance = 10f;

    [Header("Components")]
    [SerializeField] private GameObject cursorPrefab;

    private Camera mainCamera;
    private GameObject cursor;
    private Renderer cursorRenderer;
    private Material cursorMaterial;
    private bool isHoveringButton = false;

    void Start()
    {
        SetupCamera();
        CreateCursor();
    }

    void SetupCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }

        if (mainCamera == null)
        {
            Debug.LogError("No camera found for gaze cursor!");
        }
    }

    void CreateCursor()
    {
        if (cursorPrefab != null)
        {
            cursor = Instantiate(cursorPrefab);
        }
        else
        {
            // Create default sphere cursor
            cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // Remove collider to avoid interference
            Destroy(cursor.GetComponent<Collider>());
        }

        cursor.name = "GazeCursor";
        cursor.transform.localScale = Vector3.one * cursorSize;

        // Setup material
        cursorRenderer = cursor.GetComponent<Renderer>();
        if (cursorRenderer != null)
        {
            cursorMaterial = new Material(Shader.Find("Unlit/Color"));
            cursorMaterial.color = normalColor;
            cursorRenderer.material = cursorMaterial;
        }

        Debug.Log("Gaze cursor created successfully");
    }

    void Update()
    {
        if (cursor == null || mainCamera == null) return;

        UpdateCursorPosition();
        UpdateCursorColor();
    }

    void UpdateCursorPosition()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        Vector3 targetPosition;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Position on surface with offset
            targetPosition = hit.point + hit.normal * cursorDistance;

            // Check if we're hitting a button
            EyeGazeSceneButton button = hit.collider.GetComponent<EyeGazeSceneButton>();
            isHoveringButton = (button != null);

            Debug.DrawLine(ray.origin, targetPosition, Color.green);
        }
        else
        {
            // Position at max distance
            targetPosition = ray.origin + ray.direction * maxDistance;
            isHoveringButton = false;

            Debug.DrawLine(ray.origin, targetPosition, Color.red);
        }

        cursor.transform.position = targetPosition;

        // Make cursor face user consistently across both eyes
        cursor.transform.rotation = Quaternion.LookRotation(-mainCamera.transform.forward);
    }

    void UpdateCursorColor()
    {
        if (cursorMaterial == null) return;

        Color targetColor = isHoveringButton ? hoverColor : normalColor;
        cursorMaterial.color = targetColor;
    }

    // Public methods for external control
    public void SetNormalColor(Color color)
    {
        normalColor = color;
    }

    public void SetHoverColor(Color color)
    {
        hoverColor = color;
    }

    public void SetCursorSize(float size)
    {
        cursorSize = size;
        if (cursor != null)
        {
            cursor.transform.localScale = Vector3.one * size;
        }
    }

    public void SetVisible(bool visible)
    {
        if (cursor != null)
        {
            cursor.SetActive(visible);
        }
    }
}
