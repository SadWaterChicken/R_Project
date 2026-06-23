using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace New_Dungeon
{
    public class DungeonRewardManager : MonoBehaviour
    {
        private static DungeonRewardManager _instance;
        public static DungeonRewardManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<DungeonRewardManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DungeonRewardManager_Auto");
                        _instance = go.AddComponent<DungeonRewardManager>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        /// <summary>
        /// Spawns a reward chest that scales with the dungeon's current difficulty and matches its theme.
        /// </summary>
        /// <param name="position">Where to spawn</param>
        /// <param name="parent">Parent transform</param>
        /// <param name="onChestOpenedCallback">Callback to trigger when the chest is opened (e.g. for wave logic)</param>
        public void SpawnRewardChest(Vector3 position, Transform parent, UnityAction onChestOpenedCallback)
        {
            DungeonThemeSetup theme = null;
            DungeonDifficultyTier difficulty = DungeonDifficultyTier.Normal;

            if (GameStateManager.Instance != null && GameStateManager.Instance.currentTheme != null)
            {
                theme = GameStateManager.Instance.currentTheme;
                difficulty = GameStateManager.Instance.currentDifficulty;
            }
            else
            {
                RoomGenerator generator = FindAnyObjectByType<RoomGenerator>();
                if (generator != null)
                {
                    theme = generator.currentTheme;
                }
            }

            if (theme == null)
            {
                Debug.LogError("[DungeonRewardManager] Missing theme! Cannot spawn chest.");
                onChestOpenedCallback?.Invoke(); // Don't softlock the room
                return;
            }

            // 1. Roll the chest tier based on difficulty
            ChestTier tier = RollChestTier(difficulty);

            // 2. Fetch the corresponding chest data from the Theme
            ThemeChestData chestData = theme.GetChestDataByTier(tier);

            if (chestData.chestPrefab == null)
            {
                Debug.LogWarning($"[DungeonRewardManager] Theme {theme.themeName} has no chest prefab for tier {tier}! Attempting to use default.");
                // Try fallback to Common if missing
                chestData = theme.GetChestDataByTier(ChestTier.Common);
                if (chestData.chestPrefab == null)
                {
                    Debug.LogError("[DungeonRewardManager] Complete failure to find any chest prefab in theme.");
                    onChestOpenedCallback?.Invoke();
                    return;
                }
            }

            // 3. Spawn the chest
            GameObject chestObj = Instantiate(chestData.chestPrefab, position, Quaternion.identity, parent);
            
            // 4. Configure the DungeonChest component
            DungeonChest chestScript = chestObj.GetComponent<DungeonChest>();
            if (chestScript == null) 
            {
                chestScript = chestObj.AddComponent<DungeonChest>();
            }
            
            // Assign the rewards mapped in the ThemeChestData
            chestScript.possibleRewards = chestData.possibleRewards;
            
            // Hook up the event listener
            if (onChestOpenedCallback != null)
            {
                chestScript.onChestOpened.AddListener(onChestOpenedCallback);
            }
            
            Debug.Log($"[DungeonRewardManager] Spawned a {tier} chest for {difficulty} difficulty!");
        }

        private ChestTier RollChestTier(DungeonDifficultyTier difficulty)
        {
            float roll = Random.Range(0f, 100f);

            switch (difficulty)
            {
                case DungeonDifficultyTier.Easy:
                    // 70% Common, 25% Uncommon, 5% Rare
                    if (roll < 70f) return ChestTier.Common;
                    if (roll < 95f) return ChestTier.Uncommon;
                    return ChestTier.Rare;

                case DungeonDifficultyTier.Normal:
                    // 40% Common, 40% Uncommon, 15% Rare, 5% Epic
                    if (roll < 40f) return ChestTier.Common;
                    if (roll < 80f) return ChestTier.Uncommon;
                    if (roll < 95f) return ChestTier.Rare;
                    return ChestTier.Epic;

                case DungeonDifficultyTier.Hard:
                    // 30% Uncommon, 40% Rare, 20% Epic, 10% Legendary
                    if (roll < 30f) return ChestTier.Uncommon;
                    if (roll < 70f) return ChestTier.Rare;
                    if (roll < 90f) return ChestTier.Epic;
                    return ChestTier.Legendary;

                case DungeonDifficultyTier.Impossible:
                    // 20% Rare, 50% Epic, 30% Legendary
                    if (roll < 20f) return ChestTier.Rare;
                    if (roll < 70f) return ChestTier.Epic;
                    return ChestTier.Legendary;

                default:
                    return ChestTier.Common;
            }
        }
    }
}
