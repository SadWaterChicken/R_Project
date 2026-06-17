using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Persistence Data")]
    public Vector3 savedOverworldPosition;
    public DungeonThemeSetup currentTheme;
    public DungeonDifficultyTier currentDifficulty;
    public string activeDungeonInstanceID;
    public bool isBossKilled;

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
        // Make sure "DungeonScene" is added to Build Settings!
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("DungeonTesting"); // Name must match exactly
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
            SceneTransitionManager.Instance.LoadScene("PlayerTesting"); // Name must match exactly
        }
        else
        {
            Debug.LogError("[GameStateManager] No SceneTransitionManager found! Cannot load scene.");
        }
    }
}
