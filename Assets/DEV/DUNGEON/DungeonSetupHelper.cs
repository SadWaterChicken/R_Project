using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

/// <summary>
/// Helper class to create and set up the dungeon generation system
/// </summary>
public class DungeonSetupHelper
{
    [MenuItem("Dungeon/Setup New Dungeon Generation System")]
    public static void SetupDungeonSystem()
    {
        // Create RoomPrefabManager GameObject
        GameObject prefabManagerGO = new GameObject("RoomPrefabManager");
        RoomPrefabManager prefabManager = prefabManagerGO.AddComponent<RoomPrefabManager>();

        // Create DungeonGenerator GameObject
        GameObject generatorGO = new GameObject("DungeonGenerator");
        DungeonGenerator generator = generatorGO.AddComponent<DungeonGenerator>();

        // Assign prefab manager to generator
        SerializedObject generatorSO = new SerializedObject(generator);
        generatorSO.FindProperty("roomPrefabManager").objectReferenceValue = prefabManager;
        generatorSO.FindProperty("initialRoomPosition").vector3Value = Vector3.zero;
        generatorSO.FindProperty("generationSpeed").floatValue = 0.2f;
        generatorSO.FindProperty("maxRooms").intValue = 20;
        generatorSO.FindProperty("minRooms").intValue = 5;
        generatorSO.ApplyModifiedProperties();

        Debug.Log("Dungeon generation system setup complete!");
        Debug.Log("Next: Assign room prefabs to RoomPrefabManager in the Inspector");
    }

    [MenuItem("Dungeon/Find Room Prefabs")]
    public static void FindRoomPrefabs()
    {
        string[] roomPrefabGUIDs = AssetDatabase.FindAssets("Room t:Prefab", new[] { "Assets/Prefab" });
        
        Debug.Log($"Found {roomPrefabGUIDs.Length} room prefabs:");
        foreach (var guid in roomPrefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log($"  - {path}");
        }
    }
}

#endif
