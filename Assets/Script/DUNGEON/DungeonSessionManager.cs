using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static manager to track dungeon generation across scenes
/// Stores which dungeons have been generated and prevents regeneration until completed
/// </summary>
public class DungeonSessionManager : MonoBehaviour
{
    public static int currentEntranceID = -1;
    
    private static Dictionary<int, bool> generatedDungeons = new Dictionary<int, bool>(); // ID -> has been generated
    
    public static bool HasDungeonGenerated(int entranceID)
    {
        return generatedDungeons.ContainsKey(entranceID) && generatedDungeons[entranceID];
    }
    
    public static void MarkDungeonGenerated(int entranceID)
    {
        generatedDungeons[entranceID] = true;
    }
    
    public static void ClearDungeon(int entranceID)
    {
        if (generatedDungeons.ContainsKey(entranceID))
        {
            generatedDungeons.Remove(entranceID);
        }
    }
}
