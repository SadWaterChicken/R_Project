using UnityEngine;

/// <summary>
/// Represents a room instance spawned from a prefab.
/// Tracks its type, position, and possible connections.
/// </summary>
public class DungeonRoomInstance : MonoBehaviour
{
    private RoomPrefabManager.RoomType roomType;
    private Vector3Int roomSize;
    private DungeonGenerator generator;
    private Bounds roomBounds;

    public void Initialize(RoomPrefabManager.RoomType type, Vector3Int size, DungeonGenerator gen)
    {
        roomType = type;
        roomSize = size;
        generator = gen;
        
        // Calculate room bounds
        Vector3 center = transform.position + new Vector3(roomSize.x / 2f, 0, roomSize.z / 2f);
        roomBounds = new Bounds(center, new Vector3(roomSize.x, 1, roomSize.z));
    }

    public RoomPrefabManager.RoomType GetRoomType() => roomType;
    public Vector3Int GetRoomSize() => roomSize;
    public Bounds GetBounds() => roomBounds;

    /// <summary>
    /// Check if this room has an exit in a specific direction
    /// </summary>
    public bool HasExitInDirection(Direction direction)
    {
        if (generator == null || generator.GetPrefabManager() == null)
            return false;

        return generator.GetPrefabManager().HasExitInDirection(roomType, direction);
    }

    /// <summary>
    /// Get the world position of the exit in a specific direction
    /// </summary>
    public Vector3 GetExitPosition(Direction direction)
    {
        Vector3 pos = transform.position;
        Vector3Int size = roomSize;

        return direction switch
        {
            Direction.Up => pos + new Vector3(size.x / 2f, 0, size.z),
            Direction.Down => pos + new Vector3(size.x / 2f, 0, -1),
            Direction.Left => pos + new Vector3(-1, 0, size.z / 2f),
            Direction.Right => pos + new Vector3(size.x, 0, size.z / 2f),
            _ => pos
        };
    }

    /// <summary>
    /// Get the position where an adjacent room should be placed for a given direction
    /// </summary>
    public Vector3 GetAdjacentRoomPosition(Direction direction)
    {
        Vector3 pos = transform.position;
        Vector3Int size = roomSize;

        return direction switch
        {
            Direction.Up => pos + new Vector3(0, 0, size.z),
            Direction.Down => pos + new Vector3(0, 0, -size.z),
            Direction.Left => pos + new Vector3(-size.x, 0, 0),
            Direction.Right => pos + new Vector3(size.x, 0, 0),
            _ => pos
        };
    }
}
