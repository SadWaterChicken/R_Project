using UnityEngine;
using System.Collections.Generic;

public class DungeonEntitySpawner : MonoBehaviour
{
    [SerializeField] private RoomGenerator roomGenerator;
    [SerializeField] private int enemiesPerRoom = 3;
    private const float SPAWN_HEIGHT = 3f;  // Increased for thin rooms

    private void Start()
    {
        if (roomGenerator == null)
        {
            Debug.LogError("RoomGenerator not assigned!");
            return;
        }
        
        roomGenerator.onGenerationComplete += SpawnAll;
    }

    private void SpawnAll()
    {
        SpawnEnemies();
        SpawnBoss();
        Debug.Log("Entity spawning complete!");
    }

    private void SpawnEnemies()
    {
        foreach (Room room in roomGenerator.rooms)
        {
            if (room == roomGenerator.bossRoom) 
                continue;
                
            SpawnEntitiesInRoom(
                room, 
                roomGenerator.currentTheme.enemyPrefabs, 
                Random.Range(1, enemiesPerRoom + 1)
            );
        }
    }

    private void SpawnBoss()
    {
        if (roomGenerator.bossRoom == null)
            return;
        
        Room bossRoom = roomGenerator.bossRoom;
        BoxCollider collider = bossRoom.GetComponent<BoxCollider>();
        if (collider == null) return;

        Bounds bounds = collider.bounds;
        
        // Spawn boss at center of room
        Vector3 spawnPos = new Vector3(
            bounds.center.x,
            bounds.min.y + SPAWN_HEIGHT,  // Above floor
            bounds.center.z
        );

        List<GameObject> bossPrefabs = roomGenerator.currentTheme.bossPrefabs;
        if (bossPrefabs.Count > 0)
        {
            GameObject bossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Count)];
            Instantiate(bossPrefab, spawnPos, Quaternion.identity, bossRoom.transform);
            Debug.Log("Boss spawned!");
        }
    }

    private void SpawnEntitiesInRoom(Room room, List<GameObject> prefabs, int count)
    {
        if (prefabs == null || prefabs.Count == 0)
            return;

        BoxCollider collider = room.GetComponent<BoxCollider>();
        if (collider == null) 
            return;

        // Get the actual bounds of the collider (accounts for center offset)
        Bounds bounds = collider.bounds;
        
        // Add padding to avoid spawning at walls
        Vector3 padding = new Vector3(1f, 0.5f, 1f);
        Vector3 minPos = bounds.min + padding;
        Vector3 maxPos = bounds.max - padding;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(minPos.x, maxPos.x),
                bounds.min.y + SPAWN_HEIGHT,  // Above floor
                Random.Range(minPos.z, maxPos.z)
            );

            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            Instantiate(prefab, spawnPos, Quaternion.identity, room.transform);
        }
    }

    private void OnDestroy()
    {
        if (roomGenerator != null)
            roomGenerator.onGenerationComplete -= SpawnAll;
    }
}