using UnityEngine;
using System.Collections;

/// <summary>
/// Manages dungeon generation and loading for the dungeon scene
/// Uses the entrance ID as a seed to ensure consistent dungeon layouts
/// </summary>
public class DungeonManager : MonoBehaviour
{
    public DungeonGenerate dungeonGenerator;
    
    void Start()
    {
        if (dungeonGenerator == null)
        {
            dungeonGenerator = GetComponent<DungeonGenerate>();
        }
        
        // Use the entrance ID as the random seed
        Random.InitState(DungeonSessionManager.currentEntranceID);
        
        // Generate dungeon
        dungeonGenerator.GenerateDungeon();
        
        // Wait for generation to complete, then spawn player
        StartCoroutine(SpawnPlayerAfterGeneration());
        
        // Mark as generated if first time
        if (!DungeonSessionManager.HasDungeonGenerated(DungeonSessionManager.currentEntranceID))
        {
            DungeonSessionManager.MarkDungeonGenerated(DungeonSessionManager.currentEntranceID);
        }
    }
    
    IEnumerator SpawnPlayerAfterGeneration()
    {
        // Wait for next frame to ensure dungeon is fully rendered
        yield return null;
        
        // Teleport player to spawn position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 spawnPos = dungeonGenerator.GetPlayerSpawnPosition();
            spawnPos.y = 5f; // Spawn high so they land on the floor
            player.transform.position = spawnPos;
        }
    }
    
    /// <summary>
    /// Call this when the player completes the dungeon to clear it
    /// </summary>
    public void CompleteDungeon()
    {
        DungeonSessionManager.ClearDungeon(DungeonSessionManager.currentEntranceID);
    }
}
