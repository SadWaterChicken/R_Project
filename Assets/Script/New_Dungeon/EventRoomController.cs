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
        public GameObject eventChestPrefab;
        
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

            if (currentWave >= MAX_WAVES)
            {
                // Reached max waves, auto take
                TakeChestsAndEnd();
            }
            else
            {
                // Show mid-event choice
                if (eventStructure != null)
                {
                    eventStructure.ShowMidEventChoice();
                }
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

            for (int i = 0; i < numEnemies; i++)
            {
                bool spawnElite = Random.value < eliteChance;
                GameObject toSpawn = null;

                if (spawnElite && eliteEventEnemies != null && eliteEventEnemies.Count > 0)
                {
                    toSpawn = eliteEventEnemies[Random.Range(0, eliteEventEnemies.Count)];
                }
                else if (normalEventEnemies != null && normalEventEnemies.Count > 0)
                {
                    toSpawn = normalEventEnemies[Random.Range(0, normalEventEnemies.Count)];
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

        public void TakeChestsAndEnd()
        {
            isEventRunning = false;
            int chestsToSpawn = currentWave;
            
            Debug.Log($"[EventRoom] Event Finished! Spawning {chestsToSpawn} Chests.");

            // Spawn Chests
            for (int i = 0; i < chestsToSpawn; i++)
            {
                if (eventChestPrefab != null)
                {
                    Vector3 pos = transform.position + new Vector3(i * 1.5f - ((chestsToSpawn - 1) * 0.75f), 0, 0);
                    Instantiate(eventChestPrefab, pos, Quaternion.identity);
                }
            }

            if (eventStructure != null)
            {
                if (currentWave >= MAX_WAVES)
                {
                    // If maxed out, deactivate structure entirely
                    eventStructure.gameObject.SetActive(false);
                }
                else
                {
                    eventStructure.RiseUp();
                    // Optionally disable its UI so it can't be interacted with again
                    eventStructure.enabled = false; 
                }
            }

            // Unlock doors
            foreach (Door door in baseRoom.activeDoors)
            {
                door.SetLocked(false);
            }
            baseRoom.isCleared = true;
        }

        public void FailEvent()
        {
            // Called if player dies or leaves, they lose the chests.
            // Assuming player death reloads scene, this might not be needed.
            isEventRunning = false;
        }
    }
}
