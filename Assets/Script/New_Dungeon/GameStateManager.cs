using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SavedDungeonData
{
    public string dungeonInstanceID;
    public Vector3 position;
    public DungeonDifficultyTier difficulty;
}

[System.Serializable]
public class SavedDungeonDataList
{
    public List<SavedDungeonData> list = new List<SavedDungeonData>();
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Persistence Data")]
    public Vector3 savedOverworldPosition;
    public DungeonThemeSetup currentTheme;
    public DungeonDifficultyTier currentDifficulty;
    public string activeDungeonInstanceID;
    public bool isBossKilled;
    
    [Header("Overworld Portals Persistence")]
    public List<SavedDungeonData> activeDungeons = new List<SavedDungeonData>();
    private const string DUNGEON_SAVE_KEY = "SavedDungeonPortals";

    [Header("Scene Settings")]
    [Tooltip("The name of the dungeon scene to load when entering a dungeon.")]
    public string dungeonSceneName = "DungeonTesting";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadDungeons();
    }

    public void SaveDungeons()
    {
        SavedDungeonDataList data = new SavedDungeonDataList { list = activeDungeons };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(DUNGEON_SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("[GameStateManager] Saved dungeon portals: " + activeDungeons.Count);
    }

    public void LoadDungeons()
    {
        if (PlayerPrefs.HasKey(DUNGEON_SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(DUNGEON_SAVE_KEY);
            SavedDungeonDataList data = JsonUtility.FromJson<SavedDungeonDataList>(json);
            if (data != null && data.list != null)
            {
                activeDungeons = data.list;
                Debug.Log("[GameStateManager] Loaded dungeon portals: " + activeDungeons.Count);
            }
        }
    }

    public void EnterDungeon(OverworldDungeonEntrance entrance)
    {
        // Save state before entering
        // In a real scenario, you'd get the actual player position.
        // Assuming player is near the entrance:
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            savedOverworldPosition = player.transform.position;
        }
        else
        {
            savedOverworldPosition = entrance.transform.position; // Fallback
        }

        currentTheme = entrance.assignedTheme;
        currentDifficulty = entrance.GetCurrentDifficulty();
        activeDungeonInstanceID = entrance.dungeonInstanceID;
        isBossKilled = false; // Reset for new dungeon

        Debug.Log($"[GameStateManager] Entering Dungeon: {activeDungeonInstanceID}. Theme: {currentTheme.themeName}, Difficulty: {currentDifficulty}");

        // Load Dungeon Scene
        if (SceneTransitionManager.Instance != null)
        {
            if (string.IsNullOrEmpty(dungeonSceneName))
            {
                Debug.LogError("[GameStateManager] Dungeon Scene Name is empty! Please set it in the Inspector.");
                return;
            }
            SceneTransitionManager.Instance.LoadScene(dungeonSceneName);
        }
        else
        {
            Debug.LogError("[GameStateManager] No SceneTransitionManager found! Cannot load scene.");
        }
    }

    public void ReturnToOverworld()
    {
        Debug.Log($"[GameStateManager] Returning to Overworld. Boss Killed: {isBossKilled}");
        
        // Chuyển toàn bộ phần thưởng từ Dungeon Sack sang Inventory chính
        if (DungeonSack.Instance != null)
        {
            DungeonSack.Instance.TransferToInventory();
        }
        
        if (SceneTransitionManager.Instance != null)
        {
            string targetScene = SceneTransitionManager.Instance.overworldSceneName;
            if (string.IsNullOrEmpty(targetScene))
            {
                Debug.LogError("[GameStateManager] Overworld Scene Name in SceneTransitionManager is empty! Please set it in the Inspector.");
                return;
            }
            SceneTransitionManager.Instance.LoadScene(targetScene);
        }
        else
        {
            Debug.LogError("[GameStateManager] No SceneTransitionManager found! Cannot load scene.");
        }
    }
}
