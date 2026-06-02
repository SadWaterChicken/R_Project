using System.Collections.Generic;
using UnityEngine;

public enum SinType { Pride, Greed, Wrath, Envy, Lust, Gluttony, Sloth }

[System.Serializable]
public struct BossSetup
{
    public string bossName;
    public GameObject bossRoomPrefab;
    public GameObject bossPrefab;
}

[System.Serializable]
public struct ChestSetup
{
    public int level;
    public ScriptableObject chestPrefab;
    public int dropWeight;
}

/// <summary>
/// Dungeon theme configuration: enemies, bosses, rooms, and loot.
/// </summary>
[CreateAssetMenu(fileName = "NewDungeonTheme", menuName = "Dungeon/Theme Setup", order = 1)]
public class DungeonThemeSetup : ScriptableObject
{
    [Header("=== THEME ===")]
    public string themeName;
    public SinType sinType;

    [Header("=== ROOMS ===")]
    public List<GameObject> roomPrefabs;
    public List<GameObject> eventRoomPrefabs;

    [Header("=== ENEMIES ===")]
    public List<GameObject> enemyPrefabs;
    public List<GameObject> eliteEnemyPrefabs;

    [Header("=== BOSS ===")]
    public List<BossSetup> bossList;

    [Header("=== LOOT ===")]
    public List<ChestSetup> themeChests;

    [Header("=== DIFFICULTY ===")]
    [Range(0.5f, 3f)] public float enemyStatMultiplier = 1f;
    [Range(0.5f, 3f)] public float lootQualityMultiplier = 1f;
    [Range(0f, 100f)] public float eliteSpawnRate = 15f;

    #region Utility Methods

    // GetRandomBoss: returns a random boss setup from the theme
    public BossSetup GetRandomBoss()
    {
        if (bossList == null || bossList.Count == 0)
        {
            Debug.LogError($"[DungeonThemeSetup] {themeName} has no boss configuration!");
            return default;
        }
        return bossList[Random.Range(0, bossList.Count)];
    }

    // GetRandomNormalRoom: returns a random normal room GameObject
    public GameObject GetRandomNormalRoom()
    {
        var validRooms = roomPrefabs?.FindAll(r => r != null);
        return validRooms?.Count > 0 ? validRooms[Random.Range(0, validRooms.Count)] : null;
    }

    // GetRandomEventRoom: returns a random event room GameObject
    public GameObject GetRandomEventRoom()
    {
        var validRooms = eventRoomPrefabs?.FindAll(r => r != null);
        return validRooms?.Count > 0 ? validRooms[Random.Range(0, validRooms.Count)] : null;
    }

    // GetRandomEnemy: returns a random normal enemy prefab
    public GameObject GetRandomEnemy()
    {
        var validEnemies = enemyPrefabs?.FindAll(e => e != null);
        return validEnemies?.Count > 0 ? validEnemies[Random.Range(0, validEnemies.Count)] : null;
    }

    // GetRandomEliteEnemy: returns a random elite enemy prefab
    public GameObject GetRandomEliteEnemy()
    {
        var validElites = eliteEnemyPrefabs?.FindAll(e => e != null);
        return validElites?.Count > 0 ? validElites[Random.Range(0, validElites.Count)] : null;
    }

    // ShouldSpawnElite: roll chance to determine elite spawn
    public bool ShouldSpawnElite()
    {
        return Random.Range(0f, 100f) <= eliteSpawnRate;
    }

    // GetRandomEnemyWithEliteChance: choose elite or normal enemy based on chance
    public GameObject GetRandomEnemyWithEliteChance()
    {
        return ShouldSpawnElite() && GetRandomEliteEnemy() != null ? GetRandomEliteEnemy() : GetRandomEnemy();
    }

    // GetChestByLevel: returns a chest prefab matching requested level
    public ScriptableObject GetChestByLevel(int chestLevel)
    {
        if (themeChests == null || themeChests.Count == 0) return null;
        
        foreach (var chest in themeChests)
            if (chest.level == chestLevel && chest.chestPrefab != null)
                return chest.chestPrefab;
        
        return themeChests.Count > 0 ? themeChests[0].chestPrefab : null;
    }

    // ApplyDifficultyMultiplier: scale theme difficulty multipliers
    public void ApplyDifficultyMultiplier(float multiplier)
    {
        enemyStatMultiplier *= multiplier;
        lootQualityMultiplier *= multiplier;
    }

    #endregion

    // OnValidate: editor-time checks for theme configuration issues
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(themeName))
            Debug.LogWarning($"[{nameof(DungeonThemeSetup)}] Theme name is empty!");
        if ((enemyPrefabs?.Count ?? 0) == 0)
            Debug.LogWarning($"[{nameof(DungeonThemeSetup)}] {themeName} has no enemies!");
        if ((bossList?.Count ?? 0) == 0)
            Debug.LogWarning($"[{nameof(DungeonThemeSetup)}] {themeName} has no bosses!");
    }
}