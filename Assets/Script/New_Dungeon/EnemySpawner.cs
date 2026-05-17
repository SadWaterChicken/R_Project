using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;
    public int numberOfEnemiesToSpawn;
    private RoomGenerator roomGenerator;

    private void Start()
    {
        roomGenerator = GetComponent<RoomGenerator>();
        
        if (roomGenerator == null)
        {
            Debug.LogError("RoomGenerator not found on this GameObject!");
            return;
        }
        
        roomGenerator.onGenerationComplete += SpawnEnemiesInRooms;
        Debug.Log("EnemySpawner subscribed to generation complete");
    }

    void SpawnEnemiesInRooms()
    {
        Debug.Log($"Spawning enemies! Rooms count: {roomGenerator.rooms.Count}");
        
        if (roomGenerator.rooms.Count == 0)
        {
            Debug.LogWarning("No rooms to spawn enemies in!");
            return;
        }

        foreach (Room room in roomGenerator.rooms)
        {
            SpawnEnemiesInRoom(room);
        }
        
        Debug.Log("Enemy spawning complete!");
    }

    void SpawnEnemiesInRoom(Room room)
    {
        BoxCollider collider = room.GetComponent<BoxCollider>();
        if (collider == null) return;

        Vector3 roomCenter = room.transform.position;
        Vector3 roomSize = collider.size;

        int enemiesToSpawnInRoom = Random.Range(1, numberOfEnemiesToSpawn + 1);

        for (int i = 0; i < enemiesToSpawnInRoom; i++)
        {
            Vector3 randomPos = new Vector3(
                roomCenter.x + Random.Range(-roomSize.x / 2, roomSize.x / 2),
                roomCenter.y + 1f,  // Fixed height above floor
                roomCenter.z + Random.Range(-roomSize.z / 2, roomSize.z / 2)
            );

            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Instantiate(enemyPrefab, randomPos, Quaternion.identity, room.transform);
        }
    }
}