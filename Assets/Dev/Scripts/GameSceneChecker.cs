using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper để check xem game scene có đủ components cần thiết không
/// </summary>
public class GameSceneChecker : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Scene Validation")]
    [SerializeField] private bool checkOnStart = true;
    
    private void Start()
    {
        if (checkOnStart)
        {
            ValidateGameScene();
        }
    }

    [ContextMenu("Validate Game Scene")]
    public void ValidateGameScene()
    {
        Debug.Log("=== GAME SCENE VALIDATION ===");
        
        bool isValid = true;

        // 1. Check PlayerData
        PlayerData playerData = FindFirstObjectByType<PlayerData>();
        if (playerData != null)
        {
            Debug.Log($"✅ PlayerData found: {playerData.name}");
            
            // Check if PlayerData has required values
            if (playerData.CurrentHealth > 0)
            {
                Debug.Log($"   - Health: {playerData.CurrentHealth}/{playerData.MaxHealth}");
            }
            else
            {
                Debug.LogWarning("   ⚠️ PlayerData has 0 health!");
            }
        }
        else
        {
            Debug.LogError("❌ PlayerData NOT FOUND! Game will not be able to save/load.");
            isValid = false;
        }

        // 2. Check GameManager
        if (GameManager.Instance != null)
        {
            Debug.Log("✅ GameManager instance exists");
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager not found. It should persist from Menu scene.");
        }

        // 3. Check FirebaseManager
        if (FirebaseManager.Instance != null)
        {
            Debug.Log("✅ FirebaseManager instance exists");
        }
        else
        {
            Debug.LogWarning("⚠️ FirebaseManager not found. It should persist from Menu scene.");
        }

        // 4. Check SavePoints
        SavePoint[] savePoints = FindObjectsByType<SavePoint>(FindObjectsSortMode.None);
        if (savePoints.Length > 0)
        {
            Debug.Log($"✅ Found {savePoints.Length} SavePoint(s):");
            foreach (var sp in savePoints)
            {
                Debug.Log($"   - {sp.name} (ID: {sp.SavePointId})");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No SavePoints found. Players won't be able to save during gameplay.");
        }

        // 5. Check UI (Optional)
        PlayerUIController uiController = FindFirstObjectByType<PlayerUIController>();
        if (uiController != null)
        {
            Debug.Log("✅ PlayerUIController found");
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerUIController not found. UI won't display player stats.");
        }

        Debug.Log("============================");
        
        if (isValid)
        {
            Debug.Log("✅ Scene validation passed!");
        }
        else
        {
            Debug.LogError("❌ Scene validation FAILED! Fix errors above.");
        }
    }

    [ContextMenu("Auto Setup Game Scene")]
    public void AutoSetupGameScene()
    {
        Debug.Log("=== AUTO SETUP GAME SCENE ===");

        // 1. Try to find or create Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠️ No GameObject with 'Player' tag found.");
            Debug.Log("Creating placeholder 'Player' GameObject...");
            player = new GameObject("Player");
            player.tag = "Player";
        }

        // 2. Add PlayerData if missing
        PlayerData playerData = player.GetComponent<PlayerData>();
        if (playerData == null)
        {
            Debug.Log("Adding PlayerData component to Player...");
            playerData = player.AddComponent<PlayerData>();
            Debug.Log("✅ PlayerData added!");
        }
        else
        {
            Debug.Log("✅ PlayerData already exists");
        }

        // 3. Create SavePoint example
        SavePoint existingSavePoint = FindFirstObjectByType<SavePoint>();
        if (existingSavePoint == null)
        {
            Debug.Log("Creating example SavePoint...");
            GameObject savePointGO = new GameObject("SavePoint_Start");
            SavePoint savePoint = savePointGO.AddComponent<SavePoint>();
            savePointGO.transform.position = Vector3.zero;
            Debug.Log("✅ SavePoint created at origin");
        }
        else
        {
            Debug.Log("✅ SavePoint already exists");
        }

        Debug.Log("============================");
        Debug.Log("✅ Auto setup complete! Run validation to check.");
        
        #if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        #endif
    }
#endif
}
