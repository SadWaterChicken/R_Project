using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Game Manager - Quản lý flow của game, load/save, chuyển scene
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string gameSceneName = "Thinh-Testing"; // Sửa thành scene game thực tế

    [Header("Player Settings")]
    [SerializeField] private string playerId = "player_001"; // ID mặc định, có thể thay bằng login system
    [SerializeField] private int maxSaveSlots = 3; // Số slot save tối đa

    private PlayerData currentPlayerData;
    private bool isLoadingGame = false;
    private int currentSaveSlot = -1; // Slot hiện tại đang được chọn

    // Events
    public event System.Action OnGameStarted;
    public event System.Action<SaveSlotData[]> OnSaveSlotsLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager: Instance created and marked as DontDestroyOnLoad");
        }
        else
        {
            Debug.LogWarning("GameManager: Duplicate instance destroyed");
            Destroy(gameObject);
            return; // Ngừng execution nếu bị destroy
        }
    }

    private void Start()
    {
        // Đợi Firebase khởi tạo xong rồi mới set Player ID
        StartCoroutine(InitializeAfterFirebase());
    }

    private IEnumerator InitializeAfterFirebase()
    {
        // Đợi Firebase Manager được khởi tạo
        float timeout = 5f;
        float elapsed = 0f;
        
        while (FirebaseManager.Instance == null && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.SetPlayerId(playerId);
            Debug.Log("GameManager initialized with Firebase");
        }
        else
        {
            Debug.LogError("FirebaseManager not found after timeout!");
        }
    }

    /// <summary>
    /// Load tất cả save slots cho player hiện tại
    /// </summary>
    public async void LoadAllSaveSlots()
    {
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized)
        {
            Debug.LogError("Firebase not initialized!");
            return;
        }

        Debug.Log($"Loading all save slots for player: {playerId}");
        SaveSlotData[] saveSlots = new SaveSlotData[maxSaveSlots];

        for (int i = 0; i < maxSaveSlots; i++)
        {
            var slotData = await FirebaseManager.Instance.LoadSaveSlot(playerId, i);
            saveSlots[i] = slotData ?? new SaveSlotData 
            { 
                slotIndex = i, 
                isEmpty = true,
                lastSaveTime = 0,
                playerName = "Empty Slot",
                lastSavePointId = ""
            };
        }

        Debug.Log($"Loaded {saveSlots.Length} save slots, invoking OnSaveSlotsLoaded event");
        OnSaveSlotsLoaded?.Invoke(saveSlots);
    }

    /// <summary>
    /// Tạo save mới ở slot được chỉ định
    /// </summary>
    public async System.Threading.Tasks.Task CreateNewSave(int slotIndex)
    {
        if (isLoadingGame)
        {
            Debug.LogWarning("Already loading game, ignoring CreateNewSave request");
            return;
        }
        
        isLoadingGame = true;
        currentSaveSlot = slotIndex;
        
        // Set slot cho Firebase
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.SetCurrentSaveSlot(slotIndex);
        }
        
        Debug.Log($"Creating new save at slot {slotIndex}... Loading scene: {gameSceneName}");
        
        // Subscribe to scene loaded event để tạo save sau khi scene load xong
        SceneManager.sceneLoaded += OnNewGameSceneLoaded;
        
        // Load game scene
        SceneManager.LoadScene(gameSceneName);
        
        // Đợi scene load hoàn tất (chờ callback xử lý)
        await System.Threading.Tasks.Task.Delay(500);
        
        isLoadingGame = false;
    }

    /// <summary>
    /// Load game từ slot được chỉ định
    /// </summary>
    public async System.Threading.Tasks.Task LoadGameFromSlot(int slotIndex)
    {
        if (isLoadingGame) return;
        isLoadingGame = true;

        currentSaveSlot = slotIndex;
        
        // Set slot cho Firebase
        FirebaseManager.Instance.SetCurrentSaveSlot(slotIndex);

        Debug.Log($"Loading game from slot {slotIndex}...");

        var saveSlotData = await FirebaseManager.Instance.LoadSaveSlot(playerId, slotIndex);
        
        if (saveSlotData != null && !saveSlotData.isEmpty)
        {
            // Load the scene from save data, or fallback to default game scene
            string sceneToLoad = !string.IsNullOrEmpty(saveSlotData.sceneName) 
                ? saveSlotData.sceneName 
                : gameSceneName;
            
            Debug.Log($"Loading scene: {sceneToLoad} (from save data)");
            SceneManager.LoadScene(sceneToLoad);
            
            // Đợi scene load xong
            await System.Threading.Tasks.Task.Delay(100);
            
            // Load player data
            StartCoroutine(LoadPlayerDataFromSlot(saveSlotData));
        }
        else
        {
            Debug.LogError($"No save data found in slot {slotIndex}");
        }

        isLoadingGame = false;
    }

    /// <summary>
    /// Bắt đầu game mới (deprecated - dùng CreateNewSave instead)
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("Starting new game...");
        SceneManager.LoadScene(gameSceneName);
        OnGameStarted?.Invoke();
    }

    /// <summary>
    /// Teleport player to another scene and optionally to a specific SavePoint ID in that scene.
    /// This will save the current scene snapshot, then load the target scene and place the player.
    /// </summary>
    public async void TeleportToScene(string sceneName, string targetSavePointId = null)
    {
        Debug.Log($"TeleportToScene: Request to teleport to '{sceneName}' savePoint '{targetSavePointId}'");

        // Save current scene state and player
        if (SaveManager.Instance != null)
        {
            var snap = SaveManager.Instance.CaptureActiveScene();
            // we could store the snap in current save slot if desired
        }

        // Save player data to current slot before teleporting (if any)
        try
        {
            await SaveCurrentGame();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"TeleportToScene: Save failed before teleport: {e.Message}");
        }

        // Load the target scene
        SceneManager.LoadScene(sceneName);

        // Wait a short moment for scene to load
        await System.Threading.Tasks.Task.Delay(200);

        // After load, find PlayerData and place the player at the requested savepoint (if exists)
        var playerData = FindFirstObjectByType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogError("TeleportToScene: no PlayerData found in target scene after load");
            return;
        }

        // If target savepoint ID provided, find SavePoint and position player there
        if (!string.IsNullOrEmpty(targetSavePointId))
        {
            var all = GameObject.FindObjectsByType<SavePoint>(UnityEngine.FindObjectsSortMode.None);
            foreach (var sp in all)
            {
                if (sp.SavePointId == targetSavePointId)
                {
                    playerData.transform.position = sp.SpawnPosition != null ? sp.SpawnPosition.position : sp.transform.position;
                    Debug.Log($"TeleportToScene: Placed player at SavePoint {targetSavePointId}");
                    EnsurePlayerControlsEnabled(playerData);
                    OnGameStarted?.Invoke();
                    return;
                }
            }

            Debug.LogWarning($"TeleportToScene: SavePoint '{targetSavePointId}' not found in scene '{sceneName}'");
        }

        // No target specified or not found: use PlayerData.LastSavePosition if set
        if (playerData.LastSavePosition != Vector3.zero)
        {
            playerData.transform.position = playerData.LastSavePosition;
        }

        EnsurePlayerControlsEnabled(playerData);
        OnGameStarted?.Invoke();
    }

    /// <summary>
    /// Được gọi khi scene mới load xong (cho new game)
    /// </summary>
    private async void OnNewGameSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Unsubscribe để tránh gọi nhiều lần
        SceneManager.sceneLoaded -= OnNewGameSceneLoaded;

        Debug.Log($"OnNewGameSceneLoaded: Scene '{scene.name}' loaded");

        // Đợi scene initialize hoàn toàn
        await System.Threading.Tasks.Task.Delay(200);

        // Check if GameManager still exists (không bị destroy)
        if (this == null || gameObject == null)
        {
            Debug.LogError("GameManager was destroyed during scene load!");
            return;
        }

        // Tìm PlayerData và tạo save mới
        currentPlayerData = FindFirstObjectByType<PlayerData>();
        if (currentPlayerData != null)
        {
            Debug.Log($"Found PlayerData in scene: {currentPlayerData.name}");
            
            // Tạo save data ban đầu cho slot hiện tại
            if (FirebaseManager.Instance != null && currentSaveSlot >= 0)
            {
                try
                {
                    await FirebaseManager.Instance.SavePlayerDataToSlot(currentPlayerData, currentSaveSlot);
                    Debug.Log($"✅ Initial save created for slot {currentSaveSlot}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to create initial save: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Cannot save: FirebaseManager={FirebaseManager.Instance != null}, currentSaveSlot={currentSaveSlot}");
            }
        }
        else
        {
            Debug.LogError("PlayerData not found in scene!");
        }

        OnGameStarted?.Invoke();
    }

    private IEnumerator LoadPlayerDataFromSlot(SaveSlotData saveSlotData)
    {
        // Đợi 1 frame để scene load hoàn toàn
        yield return null;

        // Tìm PlayerData trong scene
        currentPlayerData = FindFirstObjectByType<PlayerData>();
        
        if (currentPlayerData != null && saveSlotData.playerDataSnapshot != null)
        {
            currentPlayerData.LoadFromSnapshot(saveSlotData.playerDataSnapshot);
            
            // Set vị trí player
            if (!string.IsNullOrEmpty(saveSlotData.lastSavePointId))
            {
                Vector3 lastPos = new Vector3(
                    saveSlotData.playerDataSnapshot.lastSavePositionX,
                    saveSlotData.playerDataSnapshot.lastSavePositionY,
                    saveSlotData.playerDataSnapshot.lastSavePositionZ
                );
                currentPlayerData.transform.position = lastPos;
            }

            Debug.Log($"Player data loaded from slot {saveSlotData.slotIndex}!");
            // Restore scene entities if snapshot present
            if (!string.IsNullOrEmpty(saveSlotData.sceneSnapshotsJson) && SaveManager.Instance != null)
            {
                var sceneSnap = SaveManager.Instance.DeserializeSceneSnapshot(saveSlotData.sceneSnapshotsJson);
                if (sceneSnap != null)
                {
                    SaveManager.Instance.RestoreScene(sceneSnap);
                }
            }
            // Ensure player control is restored in case something (pause/timeScale/components)
            // was left disabled during the load process.
            EnsurePlayerControlsEnabled(currentPlayerData);
            OnGameStarted?.Invoke();
        }
        else
        {
            Debug.LogError("PlayerData not found in scene or no save data!");
        }
    }

    /// <summary>
    /// Load game từ Firebase
    /// </summary>
    public async System.Threading.Tasks.Task LoadGame()
    {
        Debug.Log("Loading game from Firebase...");

        var snapshot = await FirebaseManager.Instance.LoadPlayerData();
        
        if (snapshot != null)
        {
            // Load game scene
            SceneManager.LoadScene(gameSceneName);
            
            // Đợi scene load xong
            await System.Threading.Tasks.Task.Delay(100);
            
            // Tìm player trong scene và load data
            StartCoroutine(LoadPlayerDataAfterSceneLoad(snapshot));
        }
        else
        {
            Debug.Log("No save data found, starting new game...");
            StartNewGame();
        }
    }

    private IEnumerator LoadPlayerDataAfterSceneLoad(PlayerDataSnapshot snapshot)
    {
        // Đợi 1 frame để scene load hoàn toàn
        yield return null;

        // Tìm PlayerData trong scene
        currentPlayerData = FindFirstObjectByType<PlayerData>();
        
        if (currentPlayerData != null)
        {
            currentPlayerData.LoadFromSnapshot(snapshot);
            Debug.Log("Player data loaded successfully!");
            EnsurePlayerControlsEnabled(currentPlayerData);
            OnGameStarted?.Invoke();
        }
        else
        {
            Debug.LogError("PlayerData not found in scene!");
        }
    }

    /// <summary>
    /// Save game hiện tại vào slot hiện tại
    /// </summary>
    public async void SaveGame()
    {
        if (currentPlayerData == null)
        {
            currentPlayerData = FindFirstObjectByType<PlayerData>();
        }

        if (currentPlayerData != null && FirebaseManager.Instance != null && currentSaveSlot >= 0)
        {
            await FirebaseManager.Instance.SavePlayerDataToSlot(currentPlayerData, currentSaveSlot);
        }
    }

    /// <summary>
    /// Save current game (async version for PauseMenu)
    /// </summary>
    public async System.Threading.Tasks.Task SaveCurrentGame()
    {
        if (currentPlayerData == null)
        {
            currentPlayerData = FindFirstObjectByType<PlayerData>();
        }

        if (currentPlayerData == null)
        {
            Debug.LogError("SaveCurrentGame: No PlayerData found!");
            throw new System.Exception("No PlayerData found to save");
        }

        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("SaveCurrentGame: FirebaseManager not available!");
            throw new System.Exception("FirebaseManager not available");
        }

        if (currentSaveSlot < 0)
        {
            Debug.LogError("SaveCurrentGame: No save slot selected!");
            throw new System.Exception("No save slot selected");
        }

        Debug.Log($"SaveCurrentGame: Saving to slot {currentSaveSlot}...");
        await FirebaseManager.Instance.SavePlayerDataToSlot(currentPlayerData, currentSaveSlot);
        Debug.Log("SaveCurrentGame: Save completed successfully");
    }

    /// <summary>
    /// Quay về menu
    /// </summary>
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    /// <summary>
    /// Thoát game
    /// </summary>
    public void QuitGame()
    {
        // Start async quit to ensure last save is attempted
        QuitAfterSaveAsync();
    }

    private async void QuitAfterSaveAsync()
    {
        // Attempt to save current game before quitting
        try
        {
            await SaveCurrentGame();
            Debug.Log("QuitAfterSaveAsync: Save completed, quitting application.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"QuitAfterSaveAsync: Save failed or unavailable: {e.Message}");
        }

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Set Player ID (dùng khi có login system)
    /// </summary>
    public void SetPlayerId(string newPlayerId)
    {
        playerId = newPlayerId;
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.SetPlayerId(playerId);
        }
    }

    public string GetPlayerId() => playerId;
    public int GetCurrentSaveSlot() => currentSaveSlot;
    public int GetMaxSaveSlots() => maxSaveSlots;
    
    private void EnsurePlayerControlsEnabled(PlayerData playerData)
    {
        if (playerData == null) return;

        // Restore time scale
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            Time.timeScale = 1f;
        }

        var go = playerData.gameObject;
        // Re-enable Rigidbody2D simulation
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null && !rb.simulated) rb.simulated = true;

        // Re-enable colliders
        var col = go.GetComponent<Collider2D>();
        if (col != null && !col.enabled) col.enabled = true;

        // Enable PlayerMovement and PlayerCombat if they exist
        var pm = go.GetComponent<PlayerMovement>();
        if (pm != null && !pm.enabled)
        {
            pm.enabled = true;
            Debug.Log("GameManager: Re-enabled PlayerMovement");
        }

        var pc = go.GetComponent<PlayerCombat>();
        if (pc != null && !pc.enabled)
        {
            pc.enabled = true;
            Debug.Log("GameManager: Re-enabled PlayerCombat");
        }
    }
}
