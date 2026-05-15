using System;

/// <summary>
/// Data structure cho mỗi save slot
/// </summary>
[System.Serializable]
public class SaveSlotData
{
    public int slotIndex;
    public bool isEmpty;
    public long lastSaveTime; // Unix timestamp
    public string playerName;
    public string lastSavePointId;
    public string sceneName; // Scene name where player was when saved
    public PlayerDataSnapshot playerDataSnapshot;
    // Serialized JSON containing scene snapshots (one or more scenes) - optional
    public string sceneSnapshotsJson;
    
    // Computed properties
    public DateTime LastSaveDateTime => DateTimeOffset.FromUnixTimeMilliseconds(lastSaveTime).LocalDateTime;
    public string FormattedLastSaveTime => isEmpty ? "Empty" : LastSaveDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    
    public SaveSlotData()
    {
        slotIndex = 0;
        isEmpty = true;
        lastSaveTime = 0;
        playerName = "Empty Slot";
        lastSavePointId = "";
        sceneName = "";
        playerDataSnapshot = null;
        sceneSnapshotsJson = "";
    }
    
    public SaveSlotData(int index, PlayerDataSnapshot snapshot, string currentSceneName = "")
    {
        slotIndex = index;
        isEmpty = false;
        lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        playerName = snapshot?.playerName ?? "Unknown";
        lastSavePointId = snapshot?.lastSavePointId ?? "";
        sceneName = currentSceneName;
        playerDataSnapshot = snapshot;
        sceneSnapshotsJson = "";
    }
}