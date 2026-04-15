using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a node in the dungeon graph before room instantiation
/// </summary>
public class DungeonNode
{
    public Vector3 Position { get; set; }
    public List<DungeonNode> Connections { get; private set; }
    public RoomPrefabManager.RoomType DeterminedRoomType { get; set; }
    public bool IsSpawned { get; set; }

    public DungeonNode(Vector3 position)
    {
        Position = position;
        Connections = new List<DungeonNode>();
        DeterminedRoomType = RoomPrefabManager.RoomType.AllDirections;
        IsSpawned = false;
    }

    public void AddConnection(DungeonNode other)
    {
        if (!Connections.Contains(other))
            Connections.Add(other);
    }

    public void RemoveConnection(DungeonNode other)
    {
        Connections.Remove(other);
    }

    public float DistanceTo(DungeonNode other)
    {
        return Vector3.Distance(Position, other.Position);
    }

    /// <summary>
    /// Determine which direction another node is relative to this node
    /// </summary>
    public Direction GetDirectionTo(DungeonNode other)
    {
        Vector3 delta = other.Position - Position;
        float absX = Mathf.Abs(delta.x);
        float absZ = Mathf.Abs(delta.z);

        if (absX > absZ)
        {
            return delta.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return delta.z > 0 ? Direction.Up : Direction.Down;
        }
    }

    /// <summary>
    /// Determine appropriate room type based on connections
    /// </summary>
    public RoomPrefabManager.RoomType DetermineRoomType()
    {
        bool hasUp = false, hasDown = false, hasLeft = false, hasRight = false;

        foreach (var connection in Connections)
        {
            Direction dir = GetDirectionTo(connection);
            switch (dir)
            {
                case Direction.Up: hasUp = true; break;
                case Direction.Down: hasDown = true; break;
                case Direction.Left: hasLeft = true; break;
                case Direction.Right: hasRight = true; break;
            }
        }

        // Determine room type based on exit directions
        if (hasUp && hasDown && hasLeft && hasRight)
            return RoomPrefabManager.RoomType.AllDirections;
        else if (hasUp && hasDown && hasLeft)
            return RoomPrefabManager.RoomType.UpDownLeft;
        else if (hasUp && hasDown && hasRight)
            return RoomPrefabManager.RoomType.UpDownRight;
        else if (hasUp && hasLeft && hasRight)
            return RoomPrefabManager.RoomType.UpLeftRight;
        else if (hasDown && hasLeft && hasRight)
            return RoomPrefabManager.RoomType.DownLeftRight;
        else if (hasUp && hasDown)
            return RoomPrefabManager.RoomType.UpDown;
        else if (hasLeft && hasRight)
            return RoomPrefabManager.RoomType.LeftRight;
        else if (hasUp && hasLeft)
            return RoomPrefabManager.RoomType.UpLeft;
        else if (hasUp && hasRight)
            return RoomPrefabManager.RoomType.UpRight;
        else if (hasDown && hasLeft)
            return RoomPrefabManager.RoomType.DownLeft;
        else if (hasDown && hasRight)
            return RoomPrefabManager.RoomType.DownRight;
        else if (hasUp)
            return RoomPrefabManager.RoomType.Up;
        else if (hasDown)
            return RoomPrefabManager.RoomType.Down;
        else if (hasLeft)
            return RoomPrefabManager.RoomType.Left;
        else if (hasRight)
            return RoomPrefabManager.RoomType.Right;
        else
            return RoomPrefabManager.RoomType.AllDirections;
    }
}
