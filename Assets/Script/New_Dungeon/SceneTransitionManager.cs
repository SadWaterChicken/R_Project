using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("UI")]
    public CanvasGroup loadingScreen;
    public Slider loadingBar;
    public float fadeDuration = 0.5f;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log($"[SceneTransitionManager] Starting load of scene: {sceneName}");
        
        // Fade to black
        if (loadingScreen != null)
        {
            loadingScreen.gameObject.SetActive(true);
            
            // Reset loading bar at the start of fade
            if (loadingBar != null)
            {
                loadingBar.value = 0f;
            }

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                loadingScreen.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }
            loadingScreen.alpha = 1f;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        float currentProgress = 0f;
        float targetProgress = 0f;

        // Wait until the asynchronous scene fully loads (Scene load is first 50% of the bar)
        while (!asyncLoad.isDone)
        {
            // asyncLoad.progress goes from 0.0 to 0.9 while loading, and 1.0 when done.
            // We clamp it and divide by 0.9f to get a smooth 0.0 to 1.0 value.
            float progressValue = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            targetProgress = progressValue * 0.5f; // Max 50% for scene load
            
            if (loadingBar != null)
            {
                // Lerp provides a buttery smooth ease-out effect
                currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 5f);
                // MoveTowards guarantees it never gets stuck mathematically
                currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * 0.5f);
                loadingBar.value = currentProgress;
            }
            
            yield return null;
        }

        // Check if there is a procedural dungeon generator in the newly loaded scene
        RoomGenerator dungeonGenerator = Object.FindAnyObjectByType<RoomGenerator>();
        if (dungeonGenerator != null)
        {
            bool isDungeonGenerated = false;
            
            // Subscribe to the generation complete event
            System.Action onComplete = () => isDungeonGenerated = true;
            dungeonGenerator.onGenerationComplete += onComplete;

            // Wait for dungeon generation to finish (second 50% of the bar)
            while (!isDungeonGenerated)
            {
                targetProgress = 0.5f + (dungeonGenerator.GetGenerationProgress() * 0.5f);
                
                if (loadingBar != null)
                {
                    currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 5f);
                    currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * 0.5f);
                    loadingBar.value = currentProgress;
                }
                yield return null;
            }

            // Unsubscribe just to be clean
            dungeonGenerator.onGenerationComplete -= onComplete;
        }

        // Ensure it reaches 100% smoothly before fading out
        targetProgress = 1f;
        while (currentProgress < 0.99f)
        {
            if (loadingBar != null)
            {
                currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 8f); // faster at the very end
                currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * 2f);
                loadingBar.value = currentProgress;
            }
            yield return null;
        }
        
        if (loadingBar != null)
        {
            loadingBar.value = 1f;
        }
        
        Debug.Log($"[SceneTransitionManager] Finished loading scene: {sceneName}");
        
        // If returning to Overworld, we handle the cleanup of the DungeonEntrance
        if (sceneName == "PlayerTesting" && GameStateManager.Instance != null)
        {
            HandleOverworldReturn();
        }

        // Fade back to clear
        if (loadingScreen != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                loadingScreen.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }
            loadingScreen.alpha = 0f;
            loadingScreen.gameObject.SetActive(false);
        }
    }
    
    private void HandleOverworldReturn()
    {
        // 1. Move player back to saved position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Move slightly offset so player isn't inside the dungeon entrance trigger immediately
            Vector3 targetPos = GameStateManager.Instance.savedOverworldPosition + new Vector3(0, 0, -2f);
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.Teleport(targetPos);
            }
            else
            {
                player.transform.position = targetPos;
            }
        }
        
        // 2. If boss was killed, find and destroy the dungeon entrance
        if (GameStateManager.Instance.isBossKilled)
        {
            OverworldDungeonEntrance[] entrances = Object.FindObjectsByType<OverworldDungeonEntrance>(FindObjectsInactive.Exclude);
            foreach(OverworldDungeonEntrance entrance in entrances)
            {
                if (entrance.dungeonInstanceID == GameStateManager.Instance.activeDungeonInstanceID)
                {
                    Debug.Log($"[SceneTransitionManager] Boss was killed. Destroying DungeonEntrance: {entrance.dungeonInstanceID}");
                    Destroy(entrance.gameObject);
                    break;
                }
            }
        }
    }
}
