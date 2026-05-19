using System.Collections.Generic;
using UnityEngine;

// ============================================================
//  RoomType
// ============================================================
public enum RoomType
{
    Start,
    Normal,
    Boss,
    Shop,
    Treasure,
    Exit
}

// ============================================================
//  RoomData
//  Dữ liệu của một phòng trong dungeon.
//  Không kế thừa MonoBehaviour — dùng như plain data class,
//  hoặc tạo ScriptableObject riêng nếu cần serialize trong Editor.
// ============================================================
[System.Serializable]
public class RoomData
{
    // -------------------------------------------------------------------------
    // Identity
    // -------------------------------------------------------------------------
    [Tooltip("ID duy nhất của phòng (ví dụ: 'room_01')")]
    public string roomId;

    [Tooltip("Tên hiển thị (dùng trên UI nếu cần)")]
    public string displayName;

    public RoomType type = RoomType.Normal;

    // -------------------------------------------------------------------------
    // Map layout
    // -------------------------------------------------------------------------
    [Tooltip("Vị trí trong lưới dungeon (đơn vị: ô grid)")]
    public Vector2Int gridPosition;

    [Tooltip("Kích thước phòng trong lưới (đơn vị: ô grid)")]
    public Vector2Int size = new Vector2Int(1, 1);

    [Tooltip("Danh sách roomId của các phòng kề được nối corridor")]
    public List<string> connectedRoomIds = new List<string>();

    // -------------------------------------------------------------------------
    // State (runtime)
    // -------------------------------------------------------------------------
    [HideInInspector] public bool isExplored = false;
    [HideInInspector] public bool isCurrentRoom = false;

    // -------------------------------------------------------------------------
    // Computed helpers
    // -------------------------------------------------------------------------
    /// <summary>Tâm ô grid của phòng (tính theo pixel trên map UI)</summary>
    public Vector2 GetMapCenter(float cellSize, float cellGap)
    {
        float step = cellSize + cellGap;
        return new Vector2(
            gridPosition.x * step + size.x * cellSize * 0.5f,
            gridPosition.y * step + size.y * cellSize * 0.5f
        );
    }

    // -------------------------------------------------------------------------
    // Constructor helpers
    // -------------------------------------------------------------------------
    public RoomData() { }

    public RoomData(string id, Vector2Int pos, RoomType roomType = RoomType.Normal)
    {
        roomId = id;
        gridPosition = pos;
        type = roomType;
    }
}