using UnityEngine;

public class RoomConnection
{
    public DungeonRoom roomA;
    public DungeonRoom roomB;
    public float distance;

    public RoomConnection(DungeonRoom a, DungeonRoom b)
    {
        roomA = a;
        roomB = b;
        distance = Vector3.Distance(a.position, b.position);
    }
}
