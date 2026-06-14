using UnityEngine;

namespace New_Dungeon
{
    [RequireComponent(typeof(Room))]
    public class BossRoomController : MonoBehaviour
    {
        private Room bossRoom;

        [Header("Boss Room Setup")]
        [Tooltip("The portal that spawns to let the player escape or go deeper.")]
        public GameObject exitPortalPrefab;
        
        // The chest that drops when the boss is defeated is now handled by DungeonRewardManager
        // public GameObject bossChestPrefab;

        private void Awake()
        {
            bossRoom = GetComponent<Room>();
            
            // Automatically mark this room as a Boss Room if the designer forgot
            bossRoom.isBossRoom = true;
            
            // Listen for the boss defeated event
            bossRoom.onBossDefeated.AddListener(HandleBossDefeated);
        }

        private void OnDestroy()
        {
            if (bossRoom != null)
            {
                bossRoom.onBossDefeated.RemoveListener(HandleBossDefeated);
            }
        }

        private void HandleBossDefeated(Room room)
        {
            if (room != bossRoom) return;

            Debug.Log($"[BossRoomController] Boss defeated in {gameObject.name}. Spawning rewards and portal.");

            // 1. Spawn Exit Portal
            if (exitPortalPrefab != null)
            {
                Vector3 portalPos = transform.position + new Vector3(0, 0.1f, 0); // Slightly above floor
                Instantiate(exitPortalPrefab, portalPos, Quaternion.identity, transform);
            }
            else
            {
                // Fallback invisible portal if designer forgot to assign one
                GameObject exitObj = new GameObject("DungeonExit_Portal");
                exitObj.transform.position = transform.position + new Vector3(0, 1.5f, 0); 
                
                BoxCollider col = exitObj.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(2f, 2f, 2f);
                
                exitObj.AddComponent<DungeonExit>();
                Debug.Log($"[BossRoomController] Spawned fallback invisible DungeonExit.");
            }

            // 2. Spawn Boss Chest using centralized Reward Manager
            Vector3 chestPos = transform.position + new Vector3(0, 0, 4f); // Offset so it's not inside the portal
            if (DungeonRewardManager.Instance != null)
            {
                // No callback needed for boss chest because wave logic is already done
                DungeonRewardManager.Instance.SpawnRewardChest(chestPos, transform, null);
            }
            else
            {
                Debug.LogError("[BossRoomController] DungeonRewardManager is missing! Cannot spawn boss chest.");
            }
        }
    }
}
