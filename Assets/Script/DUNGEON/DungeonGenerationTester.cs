using UnityEngine;

/// <summary>
/// Simple test script to validate dungeon generation prefab assignments
/// Attach this to a GameObject in the scene to run basic validation
/// </summary>
public class DungeonGenerationTester : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject floorTilePrefab;
    public GameObject wallTilePrefab;
    public GameObject doorTilePrefab;
    
    [Header("Test Settings")]
    public bool runTestOnStart = true;
    public bool generateDungeonOnTest = false;
    
    private void Start()
    {
        if (runTestOnStart)
        {
            RunValidationTest();
        }
    }
    
    public void RunValidationTest()
    {
        Debug.Log("=== DUNGEON GENERATION VALIDATION TEST ===");
        
        // Check prefab assignments
        bool prefabsValid = ValidatePrefabs();
        
        // Check DungeonGenerator component
        bool generatorValid = ValidateDungeonGenerator();
        
        // Overall result
        if (prefabsValid && generatorValid)
        {
            Debug.Log("✅ All systems validated! Dungeon generation ready.");
            
            if (generateDungeonOnTest)
            {
                StartDungeonGeneration();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Some validation checks failed. Review the logs above.");
        }
    }
    
    private bool ValidatePrefabs()
    {
        bool valid = true;
        
        Debug.Log("Checking prefab assignments...");
        
        if (floorTilePrefab == null)
        {
            Debug.LogError("❌ Floor Tile Prefab not assigned!");
            valid = false;
        }
        else
        {
            Debug.Log("✅ Floor Tile Prefab assigned");
        }
        
        if (wallTilePrefab == null)
        {
            Debug.LogError("❌ Wall Tile Prefab not assigned!");
            valid = false;
        }
        else
        {
            Debug.Log("✅ Wall Tile Prefab assigned");
        }
        
        if (doorTilePrefab == null)
        {
            Debug.LogWarning("⚠️ Door Tile Prefab not assigned (will use floor tiles)");
        }
        else
        {
            Debug.Log("✅ Door Tile Prefab assigned");
        }
        
        return valid;
    }
    
    private bool ValidateDungeonGenerator()
    {
        Debug.Log("Checking DungeonGenerator component...");
        
        DungeonGenerator generator = FindFirstObjectByType<DungeonGenerator>();
        
        if (generator == null)
        {
            Debug.LogError("❌ No DungeonGenerator found in scene!");
            return false;
        }
        else
        {
            Debug.Log("✅ DungeonGenerator found in scene");
            return true;
        }
    }
    
    private void StartDungeonGeneration()
    {
        Debug.Log("🚀 Starting dungeon generation...");
        
        DungeonGenerator generator = FindFirstObjectByType<DungeonGenerator>();
        if (generator != null)
        {
            generator.StartDungeonGeneration();
        }
    }
    
    [ContextMenu("Run Test")]
    public void RunTestFromContextMenu()
    {
        RunValidationTest();
    }
    
    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeonFromContextMenu()
    {
        StartDungeonGeneration();
    }
}