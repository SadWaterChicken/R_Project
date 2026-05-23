using UnityEngine;
using System.Collections.Generic;
using System.Collections;


[RequireComponent(typeof(Room))]
public class RoomGenerator : MonoBehaviour
{
    [SerializeField]
    List<DungeonTheme> dungeonTheme;

    public DungeonTheme currentTheme;

    [SerializeField]
    int amountOfRoomsToGenerate ;
    public List<Room> roomPrefabs;
    public Room bossRoomPrefab;
    public Room bossRoom { get; private set; }
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

        // Pick a random theme
        currentTheme = dungeonTheme[Random.Range(0, dungeonTheme.Count)];
        Debug.Log("Selected Theme: " + currentTheme.themeName + " (" + currentTheme.sin + ")");
    }

    IEnumerator Start()
    {
        StartCoroutine(GenerateRooms(roomPrefabs));
        while (generatingRoom)
            yield return new WaitForSeconds(0.5f);
        GenerateDoors();
        
        // Replace rooms after generation
        MarkFurthestRoomAsBoss();
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

    private void MarkFurthestRoomAsBoss()
{
    // BFS to find furthest room
    Dictionary<Room, int> distances = new Dictionary<Room, int>();
    Queue<Room> queue = new Queue<Room>();
    
    queue.Enqueue(generatorRoom);
    distances[generatorRoom] = 0;
    
    Room furthestRoom = generatorRoom;
    int maxDistance = 0;
    
    while (queue.Count > 0)
    {
        Room current = queue.Dequeue();
        int currentDistance = distances[current];
        
        if (currentDistance > maxDistance)
        {
            maxDistance = currentDistance;
            furthestRoom = current;
        }
        
        foreach (Room neighbor in current.GetNeighbours())
        {
            if (!distances.ContainsKey(neighbor))
            {
                distances[neighbor] = currentDistance + 1;
                queue.Enqueue(neighbor);
            }
        }
    }
    
    bossRoom = furthestRoom;
    Debug.Log($"Boss room marked at distance {maxDistance} from start");
}


    


    void Update()
    {
    }
}