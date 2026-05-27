using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private RoomGenerator roomGenerator;
    [SerializeField] private int numberOfEnemiesToSpawn = 3;
    private const float SPAWN_HEIGHT = 1.5f;

    private void Start()
    {
        roomGenerator = GetComponent<RoomGenerator>();
        if (roomGenerator == null)
        {
            Debug.LogError("[EnemySpawner] RoomGenerator not found!");
            return;
        }
        roomGenerator.onGenerationComplete += SpawnEnemiesInRooms;
    }

    private void SpawnEnemiesInRooms()
    {
        foreach (Room room in roomGenerator.rooms)
        {
            if (room == roomGenerator.bossRoom)
                SpawnBossInRoom(room);
            else
                SpawnEnemiesInRoom(room);
        }
    }

    private void SpawnBossInRoom(Room room)
    {
        BoxCollider collider = room.GetComponent<BoxCollider>();
        if (collider == null) return;

        BossSetup randomBoss = roomGenerator.currentTheme.GetRandomBoss();
        if (randomBoss.bossPrefab == null) return;

        Bounds bounds = collider.bounds;
        Vector3 bossPos = new Vector3(
            bounds.center.x,
            bounds.min.y + SPAWN_HEIGHT,
            bounds.center.z
        );

        Instantiate(randomBoss.bossPrefab, bossPos, Quaternion.identity, room.transform);
        Debug.Log($"[EnemySpawner] Boss spawned: {randomBoss.bossName}");
    }

    private void SpawnEnemiesInRoom(Room room)
    {
        BoxCollider collider = room.GetComponent<BoxCollider>();
        if (collider == null) return;

        Bounds bounds = collider.bounds;
        Vector3 padding = new Vector3(1f, 0.5f, 1f);
        Vector3 minPos = bounds.min + padding;
        Vector3 maxPos = bounds.max - padding;
        int enemiesToSpawnInRoom = Random.Range(1, numberOfEnemiesToSpawn + 1);

        for (int i = 0; i < enemiesToSpawnInRoom; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(minPos.x, maxPos.x),
                bounds.min.y + SPAWN_HEIGHT,
                Random.Range(minPos.z, maxPos.z)
            );

            GameObject enemyPrefab = roomGenerator.currentTheme.GetRandomEnemyWithEliteChance();
            if (enemyPrefab != null)
                Instantiate(enemyPrefab, randomPos, Quaternion.identity, room.transform);
        }
    }

    private void OnDestroy()
    {
        if (roomGenerator != null)
            roomGenerator.onGenerationComplete -= SpawnEnemiesInRooms;
    }
}