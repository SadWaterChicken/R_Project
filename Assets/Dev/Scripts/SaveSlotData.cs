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
    public PlayerDataSnapshot playerDataSnapshot;
    
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
        playerDataSnapshot = null;
    }
    
    public SaveSlotData(int index, PlayerDataSnapshot snapshot)
    {
        slotIndex = index;
        isEmpty = false;
        lastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        playerName = snapshot?.playerName ?? "Unknown";
        lastSavePointId = snapshot?.lastSavePointId ?? "";
        playerDataSnapshot = snapshot;
    }
}