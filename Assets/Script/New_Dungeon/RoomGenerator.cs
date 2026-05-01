using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Room))]
public class RoomGenerator : MonoBehaviour
{
    [SerializeField]
    int amountOfRoomsToGenerate = 10;
    public Room roomPrefab;

    public static readonly float prefabsDistance = 12f;
    public readonly Vector3[] offsets = new Vector3[]
    {
        Vector3.forward * prefabsDistance,   // up (Z+)
        Vector3.back * prefabsDistance,      // down (Z-)
        Vector3.left * prefabsDistance,      // left (X-)
        Vector3.right * prefabsDistance      // right (X+)
    };

    public List<Room> rooms;
    private Transform roomContainer;
    public bool generatingRoom;
    public Room generatorRoom;
    public int currentEntranceID = -1;

    private void Awake()
    {
        rooms = new List<Room>();
        generatorRoom = GetComponent<Room>();
        generatorRoom.jumpsFromStart = 0;
        roomContainer = new GameObject("Rooms").transform;
    }

    IEnumerator Start()
    {
        StartCoroutine(GenerateRooms(roomPrefab));
        while (generatingRoom)
            yield return new WaitForSeconds(0.5f);
        GenerateDoors();
    }

    private IEnumerator GenerateRooms(Room Prefab)
    {
        generatingRoom = true;
        Vector3 last = transform.position;
        int placedRooms = 0;

        while (placedRooms < amountOfRoomsToGenerate)
        {
            Room.Directions dir = (Room.Directions)Random.Range(0, 4);
            Vector3 offset = offsets[(int)dir];
            Vector3 newRoomPos = last + offset;

            Room newRoom = Instantiate(Prefab, newRoomPos, Quaternion.identity, roomContainer);
            newRoom.gameObject.name = "Room " + placedRooms;
            
            yield return new WaitForFixedUpdate(); // Let physics update
            
            // Check if colliding
            if (newRoom.collision)
            {
                Destroy(newRoom.gameObject); // Remove overlapping room
                newRoom.collision = false;
                continue; // Try again
            }
            
            rooms.Add(newRoom);
            last = newRoomPos;
            placedRooms++;
            
            yield return new WaitForSeconds(0.2f);
        }
        
        generatingRoom = false;
        yield return null;
    }

    private void GenerateDoors()
    {
        Debug.Log("GenerateDoors() called");
        generatorRoom.AssignAllNeighbours(offsets);

        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].AssignAllNeighbours(offsets);
        }
    }

    void Update()
    {
    }
}