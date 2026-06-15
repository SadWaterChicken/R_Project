using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace New_Dungeon
{
    [RequireComponent(typeof(Room))]
    public class EventRoomController : MonoBehaviour
    {
        private Room baseRoom;

        private void Awake()
        {
            baseRoom = GetComponent<Room>();
        }

        [Header("Event Settings")]
        public EventStructure eventStructure;
        // The chest spawning is now handled globally by DungeonRewardManager based on Theme and Difficulty
        // public GameObject eventChestPrefab; 
        // public List<ItemData> possibleRewards;
        
        [Header("Enemy Configuration")]
        public List<GameObject> normalEventEnemies;
        public List<GameObject> eliteEventEnemies;
        public int baseEnemiesPerWave = 3;
        public float eliteSpawnChanceIncreasePerWave = 0.2f;

        [HideInInspector] public bool isEventRunning = false;

        public int currentWave { get; private set; } = 0;
        public const int MAX_WAVES = 6;

        public void StartNextWave()
        {
            isEventRunning = true;
            currentWave++;

            Debug.Log($"[EventRoom] Starting Wave {currentWave}/{MAX_WAVES}");

            // Lock doors
            foreach (Door door in baseRoom.activeDoors)
            {
                door.SetLocked(true);
            }

            StartCoroutine(WaveRoutine());
        }

        private IEnumerator WaveRoutine()
        {
            SpawnWave();

            // Wait until all spawned enemies are dead
            yield return new WaitUntil(() => baseRoom.spawnedEnemies.Count == 0);

            Debug.Log($"[EventRoom] Wave {currentWave} cleared!");

            yield return new WaitForSeconds(1.5f);

            SpawnChest();
            
            // We do NOT RiseUp or unlock doors here. We wait for the chest to be opened!
        }

        private void SpawnChest()
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 0, 0); // Adjust as needed
            
            if (DungeonRewardManager.Instance != null)
            {
                DungeonRewardManager.Instance.SpawnRewardChest(spawnPos, transform, OnChestOpened);
            }
            else
            {
                Debug.LogError("[EventRoom] DungeonRewardManager is missing! Cannot spawn reward chest.");
                OnChestOpened(); // Auto-continue to prevent softlock
            }
        }



        public void OnChestOpened()
        {
            if (currentWave >= MAX_WAVES)
            {
                isEventRunning = false;
                if (eventStructure != null) eventStructure.gameObject.SetActive(false);
                baseRoom.isCleared = true;
            }
            else
            {
                isEventRunning = false;
                if (eventStructure != null)
                {
                    eventStructure.RiseUp();
                }
            }

            // Unlock doors after chest is opened so player can leave
            foreach (Door door in baseRoom.activeDoors)
            {
                door.SetLocked(false);
            }
        }

        private void SpawnWave()
        {
            int numEnemies = baseEnemiesPerWave + (currentWave - 1); 
            float eliteChance = (currentWave - 1) * eliteSpawnChanceIncreasePerWave;

            BoxCollider collider = baseRoom.GetComponent<BoxCollider>();
            if (collider == null) return;

            Bounds bounds = collider.bounds;
            Vector3 padding = new Vector3(1f, 0.5f, 1f);
            Vector3 minPos = bounds.min + padding;
            Vector3 maxPos = bounds.max - padding;

            DungeonThemeSetup theme = null;
            RoomGenerator generator = Object.FindAnyObjectByType<RoomGenerator>();
            if (generator != null) theme = generator.currentTheme;

            for (int i = 0; i < numEnemies; i++)
            {
                bool spawnElite = Random.value < eliteChance;
                GameObject toSpawn = null;

                if (spawnElite)
                {
                    if (eliteEventEnemies != null && eliteEventEnemies.Count > 0)
                        toSpawn = eliteEventEnemies[Random.Range(0, eliteEventEnemies.Count)];
                    else if (theme != null)
                        toSpawn = theme.GetRandomEliteEnemy();
                }

                // If not elite, or if elite prefab was missing, spawn a normal enemy
                if (toSpawn == null)
                {
                    if (normalEventEnemies != null && normalEventEnemies.Count > 0)
                        toSpawn = normalEventEnemies[Random.Range(0, normalEventEnemies.Count)];
                    else if (theme != null)
                        toSpawn = theme.GetRandomEnemy();
                }

                if (toSpawn != null)
                {
                    Vector3 spawnPos = new Vector3(
                        Random.Range(minPos.x, maxPos.x),
                        bounds.min.y + 1.5f,
                        Random.Range(minPos.z, maxPos.z)
                    );

                    GameObject enemyObj = Instantiate(toSpawn, spawnPos, Quaternion.identity, baseRoom.transform);
                    
                    RoomEnemyTracker tracker = enemyObj.GetComponent<RoomEnemyTracker>();
                    if (tracker == null)
                    {
                        tracker = enemyObj.AddComponent<RoomEnemyTracker>();
                    }
                    tracker.Initialize(baseRoom, obj => Destroy(obj), 1.0f);
                    
                    baseRoom.RegisterSpawnedEnemy(enemyObj);
                }
            }
        }

        public void FailEvent()
        {
            // Called if player dies or leaves
            isEventRunning = false;
        }
    }
}
