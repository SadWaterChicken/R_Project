using UnityEngine;
using System.Collections.Generic;
using System.Collections;


[RequireComponent(typeof(Room))]
public class RoomGenerator : MonoBehaviour
{
    [SerializeField]
    int amountOfRoomsToGenerate = 10;
    public List<Room> roomPrefabs;
    public Room bossRoomPrefab;
    public Room eventRoomPrefab;
    public System.Action onGenerationComplete;

    public static readonly float prefabsDistance = 42f;
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
        StartCoroutine(GenerateRooms(roomPrefabs));
        while (generatingRoom)
            yield return new WaitForSeconds(0.5f);
        GenerateDoors();
        
        // Replace rooms after generation
        ReplaceFurthestRoomWithBoss(bossRoomPrefab);
        onGenerationComplete?.Invoke();
    }

    private IEnumerator GenerateRooms(List<Room> Prefabs)
    {
        generatingRoom = true;
        Vector3 last = transform.position;
        int placedRooms = 0;

        while (placedRooms < amountOfRoomsToGenerate)
        {
            Room.Directions dir = (Room.Directions)Random.Range(0, 4);
            Vector3 offset = offsets[(int)dir];
            Vector3 newRoomPos = last + offset;

            // Randomly select a prefab from the list
            Room randomPrefab = Prefabs[Random.Range(0, Prefabs.Count)];
            Room newRoom = Instantiate(randomPrefab, newRoomPos, Quaternion.identity, roomContainer);
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

    // Add these methods to RoomGenerator

// Replace a room at a specific index
public void ReplaceRoomAtIndex(int index, Room bossRoomPrefab)
{
    if (index < 0 || index >= rooms.Count)
    {
        Debug.LogError("Invalid room index!");
        return;
    }

    Vector3 roomPos = rooms[index].transform.position;
    
    // Destroy old room
    Destroy(rooms[index].gameObject);
    
    // Create new room at same position
    Room newRoom = Instantiate(bossRoomPrefab, roomPos, Quaternion.identity, roomContainer);
    newRoom.gameObject.name = "BossRoom";
    
    // Replace in list
    rooms[index] = newRoom;
    
    // Reconnect doors
    newRoom.AssignAllNeighbours(offsets);
    Debug.Log($"Replaced room {index} with boss room");
    }

    // Replace the furthest room (good for boss rooms)
    public void ReplaceFurthestRoomWithBoss(Room bossRoomPrefab)
    {
        int furthestIndex = 0;
        int maxJumps = -1;
        
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].jumpsFromStart > maxJumps)
            {
                maxJumps = rooms[i].jumpsFromStart;
                furthestIndex = i;
            }
        }
        
        ReplaceRoomAtIndex(furthestIndex, bossRoomPrefab);
    }

    // Replace a random room
    public void ReplaceRandomRoom(Room specialRoomPrefab)
    {
        int randomIndex = Random.Range(0, rooms.Count);
        ReplaceRoomAtIndex(randomIndex, specialRoomPrefab);
    }

    void Update()
    {
    }
}