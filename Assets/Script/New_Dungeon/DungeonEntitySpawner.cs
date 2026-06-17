using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class DungeonEntitySpawner : MonoBehaviour
{
    [SerializeField] private RoomGenerator roomGenerator;
    [SerializeField] private int enemiesPerRoom = 3;
    private const float SPAWN_HEIGHT = 1.5f;
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> enemyPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
    private Transform poolContainer;

    private float currentStatMultiplier = 1.0f;

    // Start: initialize pool container and subscribe to generation complete
    private void Start()
    {
        if (roomGenerator == null)
        {
            Debug.LogError("[DungeonEntitySpawner] RoomGenerator not assigned!");
            return;
        }

        if (GameStateManager.Instance != null)
        {
            switch (GameStateManager.Instance.currentDifficulty)
            {
                case DungeonDifficultyTier.Easy: currentStatMultiplier = 0.8f; break;
                case DungeonDifficultyTier.Normal: currentStatMultiplier = 1.0f; break;
                case DungeonDifficultyTier.Hard: currentStatMultiplier = 1.5f; break;
                case DungeonDifficultyTier.Impossible: currentStatMultiplier = 2.5f; break;
            }
        }

        poolContainer = new GameObject("EnemyPools").transform;
        poolContainer.SetParent(transform, false);
        roomGenerator.onGenerationComplete += SpawnAll;
    }

    // SpawnAll: called after generation; subscribe rooms and spawn boss
    private void SpawnAll()
    {
        if (roomGenerator.currentTheme == null)
        {
            Debug.LogError("[DungeonEntitySpawner] No theme configured!");
            return;
        }
        
        SubscribeToRoomSpawnEvents();
        SpawnBoss();
        Debug.Log("[DungeonEntitySpawner] Spawner initialized for lazy spawning!");
    }

    private void SubscribeToRoomSpawnEvents()
    {
        int roomID = 0;
        foreach (Room room in roomGenerator.rooms)
        {
            // Skip event rooms, UNLESS it's specifically the boss room
            if (room.isEventRoom && room != roomGenerator.bossRoom) continue; 
            
            room.roomID = roomID++;
            room.onPlayerEntered -= OnRoomPlayerEntered; // Prevent double subscription just in case
            room.onPlayerEntered += OnRoomPlayerEntered;
            
            Debug.Log($"[DungeonEntitySpawner] Subscribed to room: {room.gameObject.name} (isBoss: {room == roomGenerator.bossRoom})");
        }
    }

    private void OnRoomPlayerEntered(Room room)
    {
        Debug.Log($"[DungeonEntitySpawner] OnRoomPlayerEntered called for {room.gameObject.name}. hasSpawned={room.hasSpawned}, isBossRoom={room.isBossRoom}");
        if (room.hasSpawned) return;

        if (room.isBossRoom)
        {
            Debug.Log($"[DungeonEntitySpawner] Waking up boss in {room.gameObject.name}. Enemies count: {room.spawnedEnemies.Count}");
            // The Boss was already instantiated during loading, just wake it up!
            foreach (GameObject enemy in room.spawnedEnemies)
            {
                if (enemy != null)
                {
                    Debug.Log($"[DungeonEntitySpawner] Waking up {enemy.name}");
                    RoomEnemyTracker tracker = enemy.GetComponent<RoomEnemyTracker>();
                    if (tracker != null) tracker.isSleeping = false;
                    
                    enemy.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("[DungeonEntitySpawner] Enemy in spawnedEnemies is NULL!");
                }
            }
        }
        else
        {
            // Normal rooms lazy-spawn entities from the object pool
            int count = Random.Range(1, enemiesPerRoom + 1);
            SpawnEntitiesInRoom(room, count);
        }
        
        room.hasSpawned = true;
    }

    // SpawnBoss: spawns the boss prefab in the boss room at a fixed position
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
        
        RoomEnemyTracker bossTracker = bossObj.GetComponent<RoomEnemyTracker>();
        if (bossTracker == null)
            bossTracker = bossObj.AddComponent<RoomEnemyTracker>();
            
        bossTracker.Initialize(bossRoom, obi => Destroy(bossObj), currentStatMultiplier);
        
        bossTracker.isSleeping = true; // Tell tracker we are just optimizing so it doesn't think the boss died!
        bossObj.SetActive(false); // Optimize: Keep inactive so it doesn't drain CPU before player arrives
        
        bossRoom.isBossRoom = true;
        bossRoom.RegisterSpawnedEnemy(bossObj);

        Debug.Log($"[DungeonEntitySpawner] Boss spawned: {randomBoss.bossName}");
    }

    // SpawnEntitiesInRoom: spawn `count` enemies inside given room using object pool
    private void SpawnEntitiesInRoom(Room room, int count)
    {
        BoxCollider collider = room.GetComponent<BoxCollider>();
        if (collider == null) return;

        // TÂM CỦA PHÒNG PHẢI LÀ bounds.center (Bởi vì transform.position của bạn có thể đang nằm ở góc phòng!)
        Bounds bounds = collider.bounds;
        Vector3 roomCenter = bounds.center;
        
        // Bán kính sinh quái = 80% chiều rộng của phòng để đảm bảo không bị dính vào tường
        float spawnRadius = (bounds.size.x / 2f) * 0.8f;

        for (int i = 0; i < count; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPos = new Vector3(roomCenter.x + randomCircle.x, bounds.min.y + SPAWN_HEIGHT, roomCenter.z + randomCircle.y);

            Vector3 spawnPos = randomPos;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPos = hit.position + new Vector3(0, SPAWN_HEIGHT, 0);
            }

            GameObject enemyPrefab = roomGenerator.currentTheme.GetRandomEnemyWithEliteChance();
            if (enemyPrefab != null)
            {
                ObjectPool<GameObject> pool = GetPool(enemyPrefab);
                GameObject enemyObj = pool.Get();

                // LỖI KINH ĐIỂN CỦA NAVMESH AGENT: Khi lấy ra từ Pool, NavMeshAgent bị Active ở vị trí cũ (0,0,0)
                // Nó sẽ ghi đè vị trí mới và khiến quái bị kẹt ở phòng bắt đầu!
                // CÁCH SỬA: Tạm tắt Agent đi, dịch chuyển, rồi bật lại!
                UnityEngine.AI.NavMeshAgent agent = enemyObj.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.enabled = false;
                }

                enemyObj.transform.SetParent(room.transform, false);
                enemyObj.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

                if (agent != null)
                {
                    agent.enabled = true;
                    agent.Warp(spawnPos); // Bắt NavMeshAgent nhận diện ngay lập tức vị trí mới
                }

                RoomEnemyTracker tracker = enemyObj.GetComponent<RoomEnemyTracker>();
                if (tracker == null)
                    tracker = enemyObj.AddComponent<RoomEnemyTracker>();
                tracker.Initialize(room, pool.Release, currentStatMultiplier);

                room.RegisterSpawnedEnemy(enemyObj);
            }
        }
    }

    // OnDestroy: cleanup subscriptions and listeners
    private void OnDestroy()
    {
        roomGenerator.onGenerationComplete -= SpawnAll;
        
        // Unsubscribe from room spawn events
        if (roomGenerator != null)
        {
            foreach (Room room in roomGenerator.rooms)
            {
                if (room.isEventRoom) continue;
                room.onPlayerEntered -= OnRoomPlayerEntered;
            }
        }
    }

    // GetPool: returns or creates an object pool for a given enemy prefab
    private ObjectPool<GameObject> GetPool(GameObject prefab)
    {
        if (enemyPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            return pool;

        ObjectPool<GameObject> newPool = new ObjectPool<GameObject>(
            () =>
            {
                GameObject obj = Instantiate(prefab, poolContainer);
                obj.SetActive(false);
                return obj;
            },
            obj => obj.SetActive(true),
            obj =>
            {
                obj.transform.SetParent(poolContainer, false);
                obj.SetActive(false);
            },
            obj => Destroy(obj),
            false,
            10,
            100
        );

        enemyPools.Add(prefab, newPool);
        return newPool;
    }
}