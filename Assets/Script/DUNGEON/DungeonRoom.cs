using System.Collections.Generic;
using UnityEngine;

public class DungeonRoom
{
    public Vector3 position;
    public Vector3Int size;
    public List<GameObject> tiles = new List<GameObject>();
    public int id;
    public int gridStartX = 0;
    public int gridStartZ = 0;
    public GameObject roomObject; // Reference to the room GameObject for wall creation

    public Vector3 Center => position;

    public DungeonRoom(Vector3 pos, Vector3Int roomSize, int roomId)
    {
        position = pos;
        size = roomSize;
        id = roomId;
    }

    public bool OverlapsWith(DungeonRoom other, float padding = 0.5f)
    {
        Vector3 minA = position - (Vector3)size * 0.5f - Vector3.one * padding;
        Vector3 maxA = position + (Vector3)size * 0.5f + Vector3.one * padding;

        Vector3 minB = other.position - (Vector3)other.size * 0.5f - Vector3.one * padding;
        Vector3 maxB = other.position + (Vector3)other.size * 0.5f + Vector3.one * padding;

        return !(maxA.x < minB.x || minA.x > maxB.x ||
                 maxA.y < minB.y || minA.y > maxB.y ||
                 maxA.z < minB.z || minA.z > maxB.z);
    }
}
