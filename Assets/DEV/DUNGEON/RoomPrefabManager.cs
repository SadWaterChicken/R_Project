using System.Collections.Generic;
using UnityEngine;

public class RoomPrefabManager : MonoBehaviour
{
    public enum RoomType
    {
        Up, Down, Left, Right,
        UpDown, LeftRight, UpLeft, UpRight,
        DownLeft, DownRight, UpLeftRight,
        DownLeftRight, UpDownLeft, UpDownRight, AllDirections
    }

    [Header("Room Prefabs - Assign prefabs for each room type")]
    [SerializeField] private List<GameObject> upRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> downRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> leftRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> rightRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> upDownRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> leftRightRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> upLeftRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> upRightRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> downLeftRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> downRightRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> upLeftRightRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> downLeftRightRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> upDownLeftRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> upDownRightRooms = new List<GameObject>();
    [SerializeField] private List<GameObject> allDirectionsRooms = new List<GameObject>();

    public GameObject GetRandomRoom(RoomType type)
    {
        List<GameObject> rooms = GetRoomList(type);
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning($"No prefabs for room type: {type}");
            return null;
        }
        return rooms[Random.Range(0, rooms.Count)];
    }

    private List<GameObject> GetRoomList(RoomType type)
    {
        return type switch
        {
            RoomType.Up => upRooms,
            RoomType.Down => downRooms,
            RoomType.Left => leftRooms,
            RoomType.Right => rightRooms,
            RoomType.UpDown => upDownRooms,
            RoomType.LeftRight => leftRightRooms,
            RoomType.UpLeft => upLeftRooms,
            RoomType.UpRight => upRightRooms,
            RoomType.DownLeft => downLeftRooms,
            RoomType.DownRight => downRightRooms,
            RoomType.UpLeftRight => upLeftRightRooms,
            RoomType.DownLeftRight => downLeftRightRooms,
            RoomType.UpDownLeft => upDownLeftRooms,
            RoomType.UpDownRight => upDownRightRooms,
            RoomType.AllDirections => allDirectionsRooms,
            _ => null
        };
    }

    public RoomType GetCompatibleRoomType(Direction dir)
    {
        return dir switch
        {
            Direction.Up => RoomType.Down,
            Direction.Down => RoomType.Up,
            Direction.Left => RoomType.Right,
            Direction.Right => RoomType.Left,
            _ => RoomType.AllDirections
        };
    }

    public bool HasExitInDirection(RoomType type, Direction dir)
    {
        return (type, dir) switch
        {
            (RoomType.Up, Direction.Up) => true,
            (RoomType.Down, Direction.Down) => true,
            (RoomType.Left, Direction.Left) => true,
            (RoomType.Right, Direction.Right) => true,
            (RoomType.UpDown, Direction.Up) or (RoomType.UpDown, Direction.Down) => true,
            (RoomType.LeftRight, Direction.Left) or (RoomType.LeftRight, Direction.Right) => true,
            (RoomType.UpLeft, Direction.Up) or (RoomType.UpLeft, Direction.Left) => true,
            (RoomType.UpRight, Direction.Up) or (RoomType.UpRight, Direction.Right) => true,
            (RoomType.DownLeft, Direction.Down) or (RoomType.DownLeft, Direction.Left) => true,
            (RoomType.DownRight, Direction.Down) or (RoomType.DownRight, Direction.Right) => true,
            (RoomType.UpLeftRight, Direction.Up) or (RoomType.UpLeftRight, Direction.Left) or (RoomType.UpLeftRight, Direction.Right) => true,
            (RoomType.DownLeftRight, Direction.Down) or (RoomType.DownLeftRight, Direction.Left) or (RoomType.DownLeftRight, Direction.Right) => true,
            (RoomType.UpDownLeft, Direction.Up) or (RoomType.UpDownLeft, Direction.Down) or (RoomType.UpDownLeft, Direction.Left) => true,
            (RoomType.UpDownRight, Direction.Up) or (RoomType.UpDownRight, Direction.Down) or (RoomType.UpDownRight, Direction.Right) => true,
            (RoomType.AllDirections, _) => true,
            _ => false
        };
    }

    public Vector3Int GetRoomSize(RoomType type) => new Vector3Int(12, 0, 12);
}
