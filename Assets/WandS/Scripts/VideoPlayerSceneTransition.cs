using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoPlayerSceneTransition : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("Name of the scene to load when video finishes")]
    public string targetSceneName = "MainMenu";

    [Header("Optional Settings")]
    [Tooltip("Delay in seconds before transitioning to next scene")]
    public float transitionDelay = 0f;

    [Tooltip("Fade out duration (requires a fade overlay)")]
    public float fadeOutDuration = 1f;

    private VideoPlayer videoPlayer;
    private bool hasTransitioned = false;

    void Start()
    {
        // Get the VideoPlayer component
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayerSceneTransition: No VideoPlayer component found on " + gameObject.name);
            return;
        }

        // Subscribe to the video finished event
        videoPlayer.loopPointReached += OnVideoFinished;

        // Ensure the video doesn't loop
        videoPlayer.isLooping = false;

        Debug.Log("VideoPlayerSceneTransition: Initialized. Will transition to scene '" + targetSceneName + "' when video finishes.");
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (hasTransitioned)
            return;

        hasTransitioned = true;

        Debug.Log("VideoPlayerSceneTransition: Video finished playing. Transitioning to scene: " + targetSceneName);

        if (transitionDelay > 0)
        {
            // Use a coroutine for delayed transition
            StartCoroutine(TransitionAfterDelay());
        }
        else
        {
            // Immediate transition
            TransitionToScene();
        }
    }

    private System.Collections.IEnumerator TransitionAfterDelay()
    {
        yield return new WaitForSeconds(transitionDelay);
        TransitionToScene();
    }

    private void TransitionToScene()
    {
        // Validate scene name before loading
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("VideoPlayerSceneTransition: Target scene name is empty!");
            return;
        }

        // Check if scene exists in build settings
        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("VideoPlayerSceneTransition: Scene '" + targetSceneName + "' not found in build settings!");

            // Fallback: try loading by build index 0 (usually main menu)
            if (SceneManager.sceneCountInBuildSettings > 0)
            {
                Debug.Log("VideoPlayerSceneTransition: Loading scene at build index 0 as fallback.");
                SceneManager.LoadScene(0);
            }
        }
    }

    // Optional: Allow manual scene transition (useful for testing or skip functionality)
    public void ManualTransition()
    {
        if (!hasTransitioned)
        {
            OnVideoFinished(videoPlayer);
        }
    }

    // Optional: Method to change target scene at runtime
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        Debug.Log("VideoPlayerSceneTransition: Target scene changed to: " + sceneName);
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}