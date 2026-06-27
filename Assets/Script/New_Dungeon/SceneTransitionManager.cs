using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    
    [Header("Overworld Settings")]
    [Tooltip("Type the name of the scene you are working on to return to it when leaving the dungeon.")]
    public string overworldSceneName;

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
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneTransitionManager] Attempted to load a scene, but the scene name provided was empty!");
            return;
        }

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
            float progressValue = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            targetProgress = progressValue * 0.5f; // Max 50% for scene load
            
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 5f);
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * 0.5f);

            if (loadingBar != null)
            {
                loadingBar.value = currentProgress;
            }
            
            yield return null;
        }

        // Check if there is a procedural dungeon generator in the newly loaded scene
        RoomGenerator dungeonGenerator = Object.FindAnyObjectByType<RoomGenerator>();
        if (dungeonGenerator != null)
        {
            bool isDungeonGenerated = false;
            
            System.Action onComplete = () => isDungeonGenerated = true;
            dungeonGenerator.onGenerationComplete += onComplete;

            // Wait for dungeon generation to finish (second 50% of the bar)
            while (!isDungeonGenerated)
            {
                targetProgress = 0.5f + (dungeonGenerator.GetGenerationProgress() * 0.5f);
                
                currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 5f);
                currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * 0.5f);

                if (loadingBar != null)
                {
                    loadingBar.value = currentProgress;
                }
                yield return null;
            }

            dungeonGenerator.onGenerationComplete -= onComplete;
        }

        // Ensure it reaches 100% smoothly before fading out
        targetProgress = 1f;
        while (currentProgress < 0.99f)
        {
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 8f); // faster at the very end
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * 2f);

            if (loadingBar != null)
            {
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
        if (sceneName == overworldSceneName && GameStateManager.Instance != null)
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
