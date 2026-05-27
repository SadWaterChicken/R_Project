using UnityEngine;
using System.Collections.Generic;

public class DungeonEntitySpawner : MonoBehaviour
{
    [SerializeField] private RoomGenerator roomGenerator;
    [SerializeField] private int enemiesPerRoom = 3;
    private const float SPAWN_HEIGHT = 1.5f;

    private void Start()
    {
        if (roomGenerator == null)
        {
            Debug.LogError("[DungeonEntitySpawner] RoomGenerator not assigned!");
            return;
        }
        roomGenerator.onGenerationComplete += SpawnAll;
    }

    private void SpawnAll()
    {
        if (roomGenerator.currentTheme == null)
        {
            Debug.LogError("[DungeonEntitySpawner] No theme configured!");
            return;
        }
        
        SpawnEnemies();
        SpawnBoss();
        Debug.Log("[DungeonEntitySpawner] Entity spawning complete!");
    }

    private void SpawnEnemies()
    {
        foreach (Room room in roomGenerator.rooms)
        {
            if (room == roomGenerator.bossRoom) continue;
            
            int count = Random.Range(1, enemiesPerRoom + 1);
            SpawnEntitiesInRoom(room, count);
        }
    }

    private void SpawnBoss()
    {
        if (roomGenerator.bossRoom == null) return;
        
        Room bossRoom = roomGenerator.bossRoom;
        BoxCollider collider = bossRoom.GetComponent<BoxCollider>();
        if (collider == null) return;

        BossSetup randomBoss = roomGenerator.currentTheme.GetRandomBoss();
        if (randomBoss.bossPrefab == null) return;

        Bounds bounds = collider.bounds;
        Vector3 spawnPos = new Vector3(
            bounds.center.x,
            bounds.min.y + SPAWN_HEIGHT,
            bounds.center.z
        );

        GameObject bossObj = Instantiate(randomBoss.bossPrefab, spawnPos, Quaternion.identity, bossRoom.transform);
        bossRoom.isBossRoom = true;
        bossRoom.isCleared = false;
        bossRoom.spawnedEnemies.Add(bossObj);
        
        Debug.Log($"[DungeonEntitySpawner] Boss spawned: {randomBoss.bossName}");
    }

    private void SpawnEntitiesInRoom(Room room, int count)
    {
        BoxCollider collider = room.GetComponent<BoxCollider>();
        if (collider == null) return;

        Bounds bounds = collider.bounds;
        Vector3 padding = new Vector3(1f, 0.5f, 1f);
        Vector3 minPos = bounds.min + padding;
        Vector3 maxPos = bounds.max - padding;

        bool spawnedAny = false;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(minPos.x, maxPos.x),
                bounds.min.y + SPAWN_HEIGHT,
                Random.Range(minPos.z, maxPos.z)
            );

            GameObject enemyPrefab = roomGenerator.currentTheme.GetRandomEnemyWithEliteChance();
            if (enemyPrefab != null)
            {
                GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, room.transform);
                room.spawnedEnemies.Add(enemyObj);
                spawnedAny = true;
            }
        }

        if (spawnedAny)
        {
            room.isCleared = false;
        }
    }

    private void OnDestroy()
    {
        if (roomGenerator != null)
            roomGenerator.onGenerationComplete -= SpawnAll;
    }
}