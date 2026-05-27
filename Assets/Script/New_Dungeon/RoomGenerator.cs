using UnityEngine;
using System.Collections.Generic;
using System.Collections;


[RequireComponent(typeof(Room))]
public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private List<DungeonThemeSetup> dungeonThemes;
    public DungeonThemeSetup currentTheme { get; private set; }

    [SerializeField] private int amountOfRoomsToGenerate = 10;
    [SerializeField] private int minEventRooms = 1;
    [SerializeField] private int maxEventRooms = 3;
    
    private List<Room> roomPrefabs;
    private List<Room> eventRoomPrefabs;
    public Room bossRoom { get; private set; }
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

        if (dungeonThemes == null || dungeonThemes.Count == 0)
        {
            Debug.LogError("[RoomGenerator] No dungeon themes assigned!");
            return;
        }

        currentTheme = dungeonThemes[Random.Range(0, dungeonThemes.Count)];
        
        // Convert regular room prefabs
        roomPrefabs = new List<Room>(currentTheme.roomPrefabs.ConvertAll(go => go.GetComponent<Room>()));
        roomPrefabs.RemoveAll(r => r == null);
        
        // Convert event room prefabs
        eventRoomPrefabs = new List<Room>(currentTheme.eventRoomPrefabs.ConvertAll(go => go.GetComponent<Room>()));
        eventRoomPrefabs.RemoveAll(r => r == null);
        
        Debug.Log($"[RoomGenerator] Selected Theme: {currentTheme.themeName} ({currentTheme.sinType})");
        Debug.Log($"[RoomGenerator] Regular rooms: {roomPrefabs.Count}, Event rooms: {eventRoomPrefabs.Count}");
    }

    IEnumerator Start()
    {
        if (currentTheme == null || roomPrefabs.Count == 0)
        {
            Debug.LogError("[RoomGenerator] Cannot start: theme or rooms not initialized!");
            yield break;
        }
        
        yield return StartCoroutine(GenerateRooms());
        GenerateDoors();
        MarkFurthestRoomAsBoss();
        onGenerationComplete?.Invoke();
    }

    private IEnumerator GenerateRooms()
    {
        generatingRoom = true;
        Vector3 lastPos = transform.position;
        int placedRooms = 0;
        
        // Determine how many event rooms to spawn (1-3)
        int eventRoomsToSpawn = Random.Range(minEventRooms, maxEventRooms + 1);
        int eventRoomsSpawned = 0;
        
        // If no event rooms available, just generate regular rooms
        if (eventRoomPrefabs == null || eventRoomPrefabs.Count == 0)
            eventRoomsToSpawn = 0;

        while (placedRooms < amountOfRoomsToGenerate)
        {
            Vector3 offset = offsets[Random.Range(0, offsets.Length)];
            Vector3 newRoomPos = lastPos + offset;
            
            // Decide whether to spawn event room or regular room
            bool shouldSpawnEvent = eventRoomsSpawned < eventRoomsToSpawn && Random.value > 0.6f;
            
            Room randomPrefab;
            if (shouldSpawnEvent && eventRoomPrefabs.Count > 0)
            {
                randomPrefab = eventRoomPrefabs[Random.Range(0, eventRoomPrefabs.Count)];
            }
            else
            {
                randomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            }
            
            Room newRoom = Instantiate(randomPrefab, newRoomPos, Quaternion.identity, roomContainer);
            string roomType = shouldSpawnEvent && eventRoomPrefabs.Count > 0 ? "Event" : "Regular";
            newRoom.gameObject.name = $"Room_{placedRooms}_{roomType}";
            
            yield return new WaitForFixedUpdate();
            
            if (newRoom.collision)
            {
                Destroy(newRoom.gameObject);
                newRoom.collision = false;
                continue;
            }
            
            rooms.Add(newRoom);
            if (shouldSpawnEvent && eventRoomPrefabs.Count > 0)
                eventRoomsSpawned++;
            
            lastPos = newRoomPos;
            placedRooms++;
            yield return new WaitForSeconds(0.1f);
        }
        
        // If we didn't spawn enough event rooms, add them at the end
        if (eventRoomsSpawned < minEventRooms && eventRoomPrefabs.Count > 0)
        {
            Debug.LogWarning($"[RoomGenerator] Spawned {eventRoomsSpawned} event rooms but needed {minEventRooms}. Adding more...");
            for (int i = eventRoomsSpawned; i < minEventRooms; i++)
            {
                Vector3 offset = offsets[Random.Range(0, offsets.Length)];
                Vector3 newRoomPos = lastPos + offset;
                Room eventPrefab = eventRoomPrefabs[Random.Range(0, eventRoomPrefabs.Count)];
                Room newRoom = Instantiate(eventPrefab, newRoomPos, Quaternion.identity, roomContainer);
                newRoom.gameObject.name = $"Room_{placedRooms}_Event";
                rooms.Add(newRoom);
                lastPos = newRoomPos;
                placedRooms++;
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        Debug.Log($"[RoomGenerator] Generated {placedRooms} rooms with {eventRoomsSpawned} event rooms");
        generatingRoom = false;
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
    // NEW: Replace the furthest room with boss arena
    Vector3 bossRoomPos = bossRoom.transform.position;
    Destroy(bossRoom.gameObject);
    rooms.Remove(bossRoom);
    
    BossSetup randomBoss = currentTheme.GetRandomBoss();
    if (randomBoss.bossRoomPrefab != null)
    {
        bossRoom = Instantiate(
            randomBoss.bossRoomPrefab, 
            bossRoomPos, 
            Quaternion.identity, 
            roomContainer
        ).GetComponent<Room>();
        rooms.Add(bossRoom);
        Debug.Log($"[RoomGenerator] Boss arena spawned: {randomBoss.bossName}");
    }
}


    



}