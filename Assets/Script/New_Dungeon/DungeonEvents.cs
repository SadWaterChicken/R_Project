using System;

public static class DungeonEvents
{
    public static event Action<Room> OnBossDefeated;
    public static event Action<Room> OnRoomCleared;
    public static event Action<Room> OnPlayerEnteredRoom;
    public static event Action<DungeonBuff> OnBuffApplied;

    // RaiseBossDefeated: notify subscribers that a boss was defeated in `r`
    public static void RaiseBossDefeated(Room r) => OnBossDefeated?.Invoke(r);
    
    // RaiseRoomCleared: notify subscribers that room `r` was cleared
    public static void RaiseRoomCleared(Room r) => OnRoomCleared?.Invoke(r);
    
    // RaisePlayerEnteredRoom: notify subscribers that player entered room `r`
    public static void RaisePlayerEnteredRoom(Room r) => OnPlayerEnteredRoom?.Invoke(r);

    // RaiseBuffApplied: notify UI and systems that a buff was applied to player
    public static void RaiseBuffApplied(DungeonBuff buff) => OnBuffApplied?.Invoke(buff);
}
