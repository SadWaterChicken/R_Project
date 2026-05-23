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
                    Debug.LogError("RoomGenerator not found!");
                    return;
                }
                roomGenerator.onGenerationComplete += SpawnEnemiesInRooms;
            }

            void SpawnEnemiesInRooms()
            {
                foreach (Room room in roomGenerator.rooms)
                {
                    if (room == roomGenerator.bossRoom)
                        SpawnBossInRoom(room);
                    else
                        SpawnEnemiesInRoom(room);
                }
            }

            void SpawnBossInRoom(Room room)
            {
                BoxCollider collider = room.GetComponent<BoxCollider>();
                if (collider == null) return;

                Vector3 roomCenter = room.transform.position;
                Vector3 roomSize = collider.size;
                
                // Use boss prefabs from current theme
                List<GameObject> bossPrefabs = roomGenerator.currentTheme.bossPrefabs;
                if (bossPrefabs.Count == 0) return;

                Vector3 bossPos = new Vector3(
                    roomCenter.x,
                    roomCenter.y + 1f,
                    roomCenter.z
                );

                GameObject bossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Count)];
                Instantiate(bossPrefab, bossPos, Quaternion.identity, room.transform);
                Debug.Log("Boss spawned!");
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