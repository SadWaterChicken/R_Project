using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
        public GameObject eventChestPrefab;
        public DungeonEntitySpawner spawner; // Reference to existing spawner in room
        
        [Header("Enemy Prefabs for Event")]
        public List<GameObject> normalEventEnemies;
        public List<GameObject> eliteEventEnemies;

        [HideInInspector] public bool isEventRunning = false;

        private int hiddenScore = 0;
        private int currentWave = 0;
        private int totalWaves = 0;
        private int enemiesAliveInWave = 0;

        public void StartEvent(int cubesOffered)
        {
            if (isEventRunning) return;

            isEventRunning = true;
            hiddenScore = 0;
            currentWave = 0;

            int points = cubesOffered * 5;

            // Calculate Threshold
            if (points >= 300) totalWaves = 3;
            else if (points >= 200) totalWaves = 2;
            else totalWaves = 1;

            Debug.Log($"[EventRoom] Event Started! Cubes: {cubesOffered}, Points: {points}, Waves: {totalWaves}");

            // Lock doors
            foreach (Door door in baseRoom.activeDoors)
            {
                door.SetLocked(true);
            }

            StartNextWave();
        }

        private void StartNextWave()
        {
            if (currentWave >= totalWaves)
            {
                EndEvent();
                return;
            }

            currentWave++;
            Debug.Log($"[EventRoom] Starting Wave {currentWave}/{totalWaves}");

            // Example spawn logic
            int numEnemies = 3 + currentWave * 2; // Arbitrary formula for testing
            enemiesAliveInWave = numEnemies;

            for (int i = 0; i < numEnemies; i++)
            {
                bool spawnElite = false;
                
                // Higher waves = more elites
                if (currentWave == 1)
                {
                    spawnElite = Random.value < 0.2f; // 20% elite
                }
                else if(currentWave == 2)
                {
                    spawnElite = Random.value < 0.5f; // 50% elite in wave 2
                }
                else if(currentWave == 3)
                {
                    spawnElite = Random.value < 0.7f; // 70% elite in wave 3
                }

                GameObject toSpawn = null;
                int scoreValue = 5;

                if (spawnElite && eliteEventEnemies.Count > 0)
                {
                    toSpawn = eliteEventEnemies[Random.Range(0, eliteEventEnemies.Count)];
                    scoreValue = 10;
                }
                else if (normalEventEnemies.Count > 0)
                {
                    toSpawn = normalEventEnemies[Random.Range(0, normalEventEnemies.Count)];
                    scoreValue = 5;
                }

                if (toSpawn != null)
                {
                    // Call spawner (assuming you have a method to spawn at random points)
                    // We mock spawn point here
                    Vector3 spawnPos = transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                    
                    GameObject enemyObj = Instantiate(toSpawn, spawnPos, Quaternion.identity);
                    
                    // Add a script to detect death and report to this room
                    EventEnemyTracker tracker = enemyObj.AddComponent<EventEnemyTracker>();
                    tracker.Initialize(this, scoreValue);
                    
                    // TODO: If enemyObj has a normal Drop script, disable it here 
                    // so it doesn't drop normal items.
                    
                    baseRoom.RegisterSpawnedEnemy(enemyObj);
                }
            }
        }

        public void OnEventEnemyDied(GameObject enemy, int scoreValue)
        {
            hiddenScore += scoreValue;
            enemiesAliveInWave--;

            // Handle drops (Buff items only)
            // RollDrop();

            baseRoom.OnEnemyDied(enemy); // Call base room logic

            if (enemiesAliveInWave <= 0)
            {
                StartCoroutine(WaitAndStartNextWave());
            }
        }

        private IEnumerator WaitAndStartNextWave()
        {
            yield return new WaitForSeconds(2f);
            StartNextWave();
        }

        private void EndEvent()
        {
            isEventRunning = false;
            Debug.Log($"[EventRoom] Event Finished! Hidden Score: {hiddenScore}");

            // Spawn Chest based on Hidden Score
            int numChests = 1 + (hiddenScore / 50); // E.g., 100 score = 3 chests
            
            for (int i=0; i<numChests; i++)
            {
                if (eventChestPrefab != null)
                {
                    Vector3 pos = transform.position + new Vector3(i * 1.5f, 0, 0);
                    Instantiate(eventChestPrefab, pos, Quaternion.identity);
                }
                else
                {
                    Debug.Log($"[EventRoom] Spawning Mock Chest {i+1}!");
                }
            }

            // Restore structure
            if (eventStructure != null)
            {
                eventStructure.RiseUp();
            }

            // Unlock doors (handled by base Room if spawnedEnemies.Count == 0, but we do it manually to be safe)
            foreach (Door door in baseRoom.activeDoors)
            {
                door.SetLocked(false);
            }
            baseRoom.isCleared = true;
        }
    }

    // Helper script attached to event enemies to track death and points
    public class EventEnemyTracker : MonoBehaviour
    {
        private EventRoomController room;
        private int score;
        private bool isQuitting = false;

        public void Initialize(EventRoomController room, int score)
        {
            this.room = room;
            this.score = score;
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        private void OnDestroy()
        {
            if (isQuitting || !gameObject.scene.isLoaded) return;
            if (room != null)
            {
                room.OnEventEnemyDied(gameObject, score);
            }
        }
    }
}
