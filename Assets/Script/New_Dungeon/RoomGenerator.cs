using UnityEngine;
using System.Collections.Generic;
using System.Collections;


[RequireComponent(typeof(Room))]
public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private List<DungeonThemeSetup> dungeonThemes;
    public DungeonThemeSetup currentTheme { get; private set; }

    [SerializeField] private int normalRoomsToGenerate = 10;
    [SerializeField] private int minEventRooms = 1;
    [SerializeField] private int maxEventRooms = 3;
    
    private List<Room> roomPrefabs;
    private List<Room> eventRoomPrefabs;
    public Room bossRoom { get; private set; }
    public System.Action onGenerationComplete;
    
    private float roomSpacing = 42f;
    private Vector3[] offsets;

    public List<Room> rooms;
    public Dictionary<Vector2Int, Room> roomGrid;
    private Transform roomContainer;
    public bool generatingRoom;
    public Room generatorRoom;
    public int currentEntranceID = -1;

    // Awake: initialize generator state, offsets, and selected theme
    private void Awake()
    {
        rooms = new List<Room>();
        roomGrid = new Dictionary<Vector2Int, Room>();
        
        generatorRoom = GetComponent<Room>();
        generatorRoom.jumpsFromStart = 0;
        roomGrid[Vector2Int.zero] = generatorRoom;
        
        roomContainer = new GameObject("Rooms").transform;
        roomContainer.SetParent(this.transform); // Make it a child of the generator so NavMesh can easily target it

        BoxCollider generatorCollider = generatorRoom.GetComponent<BoxCollider>();
        if (generatorCollider != null)
        {
            Vector3 size = generatorCollider.size;
            roomSpacing = Mathf.Max(size.x, size.z);
        }

        offsets = new Vector3[]
        {
            Vector3.forward * roomSpacing,   // up (Z+)
            Vector3.back * roomSpacing,      // down (Z-)
            Vector3.left * roomSpacing,      // left (X-)
            Vector3.right * roomSpacing      // right (X+)
        };

        if (GameStateManager.Instance != null && GameStateManager.Instance.currentTheme != null)
        {
            currentTheme = GameStateManager.Instance.currentTheme;
            
            switch (GameStateManager.Instance.currentDifficulty)
            {
                case DungeonDifficultyTier.Normal: normalRoomsToGenerate += 5; break;
                case DungeonDifficultyTier.Hard: normalRoomsToGenerate += 10; break;
                case DungeonDifficultyTier.Impossible: normalRoomsToGenerate += 15; break;
            }
        }
        else
        {
            // Ensure we only use valid themes for fallback
            if (dungeonThemes != null)
            {
                dungeonThemes.RemoveAll(theme => theme == null);
            }

            if (dungeonThemes != null && dungeonThemes.Count > 0)
            {
                currentTheme = dungeonThemes[Random.Range(0, dungeonThemes.Count)];
            }
            else
            {
                Debug.LogError("[RoomGenerator] No dungeon themes assigned in GameStateManager OR RoomGenerator fallback list!");
                return;
            }
        }
        
        if (currentTheme == null)
        {
            Debug.LogError("[RoomGenerator] Selected theme is null!");
            return;
        }

        // Convert regular room prefabs safely
        roomPrefabs = new List<Room>();
        if (currentTheme.roomPrefabs != null)
        {
            foreach (var go in currentTheme.roomPrefabs)
            {
                if (go != null)
                {
                    Room r = go.GetComponent<Room>();
                    if (r != null) roomPrefabs.Add(r);
                }
            }
        }
        
        // Convert event room prefabs safely
        eventRoomPrefabs = new List<Room>();
        if (currentTheme.eventRoomPrefabs != null)
        {
            foreach (var go in currentTheme.eventRoomPrefabs)
            {
                if (go != null)
                {
                    Room r = go.GetComponent<Room>();
                    if (r != null) eventRoomPrefabs.Add(r);
                }
            }
        }
        
        Debug.Log($"[RoomGenerator] Selected Theme: {currentTheme.themeName} ({currentTheme.sinType})");
        Debug.Log($"[RoomGenerator] Regular rooms: {roomPrefabs.Count}, Event rooms: {eventRoomPrefabs.Count}");
    }

    // Start: coroutine entry point to run generation sequence
    IEnumerator Start()
    {
        if (currentTheme == null || roomPrefabs.Count == 0)
        {
            Debug.LogError("[RoomGenerator] Cannot start: theme or rooms not initialized!");
            yield break;
        }
        
        yield return StartCoroutine(GenerateRooms());
        MarkFurthestRoomAsBoss();
        
        // Generate all doors AFTER all rooms (including Boss Room) are placed
        GenerateDoors();
        
        MarkGenerationComplete();
        
        // Set the starting room as active by default
        generatorRoom.SetRoomActive(true);

        // Teleport the player to the start room
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Place player slightly above the floor to avoid clipping
            Vector3 targetPos = generatorRoom.transform.position + new Vector3(0, 1f, 0);
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.Teleport(targetPos);
            }
            else
            {
                player.transform.position = targetPos;
            }
            Debug.Log("[RoomGenerator] Teleported player to starting room.");
        }
        else
        {
            Debug.LogWarning("[RoomGenerator] Player not found! Could not teleport to start room.");
        }
        
        // --- NAVMESH BAKING ---
        Unity.AI.Navigation.NavMeshSurface surface = GetComponent<Unity.AI.Navigation.NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
            Debug.Log("[RoomGenerator] NavMesh baked successfully at runtime!");
        }
        else
        {
            Debug.LogWarning("[RoomGenerator] NavMeshSurface component missing! Cannot bake NavMesh.");
        }    
        
        // Optimize Performance: Batch all room geometry into a single static mesh
        // MUST BE DONE AFTER NAVMESH BAKING, otherwise the combined mesh blocks CPU read access.
        // TẠM THỜI TẮT LỆNH NÀY: Lệnh này gom tất cả model thành 1 cục tĩnh (Static) để nhẹ máy.
        // NHƯNG nó làm cho các vật thể có Animation di chuyển (như EventStructure thụt xuống/trồi lên) bị đóng băng!
        // StaticBatchingUtility.Combine(roomContainer.gameObject);
        
        onGenerationComplete?.Invoke();
    }

    // MarkGenerationComplete: flip generationComplete on all rooms
    private void MarkGenerationComplete()
    {
        generatorRoom.generationComplete = true;
        foreach (Room room in rooms)
        {
            room.generationComplete = true;
        }
    }

    // GenerateRooms: procedural placement loop for rooms and event rooms
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

        int totalRoomsToGenerate = normalRoomsToGenerate + 1;
        int attempts = 0;
        int maxAttempts = 15;

        while (placedRooms < totalRoomsToGenerate)
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
            bool isEvent = shouldSpawnEvent && eventRoomPrefabs.Count > 0;
            string roomType = isEvent ? "Event" : "Regular";
            newRoom.isEventRoom = isEvent;
            newRoom.gameObject.name = $"Room_{placedRooms}_{roomType}";

            if (IsRoomOverlapping(newRoomPos))
            {
                Destroy(newRoom.gameObject);
                attempts++;
                if (attempts > maxAttempts)
                {
                    if (rooms.Count > 0)
                        lastPos = rooms[Random.Range(0, rooms.Count)].transform.position;
                    else
                        lastPos = transform.position;
                    attempts = 0;
                }
                yield return null; // Prevenet Editor freezing
                continue;
            }
            
            attempts = 0;
            
            rooms.Add(newRoom);
            roomGrid[WorldToGridPosition(newRoomPos)] = newRoom;
            
            if (shouldSpawnEvent && eventRoomPrefabs.Count > 0)
                eventRoomsSpawned++;
            
            // Deferred door connection to the end
            // newRoom.AssignAllNeighbours(offsets, roomSpacing);
            // generatorRoom.AssignAllNeighbours(offsets, roomSpacing);
            
            lastPos = newRoomPos;
            placedRooms++;
            yield return null;
        }
        
        // If we didn't spawn enough event rooms, add them at the end
        if (eventRoomsSpawned < minEventRooms && eventRoomPrefabs.Count > 0)
        {
            Debug.LogWarning($"[RoomGenerator] Spawned {eventRoomsSpawned} event rooms but needed {minEventRooms}. Adding more...");
            int eventAttempts = 0;
            while (eventRoomsSpawned < minEventRooms)
            {
                Vector3 offset = offsets[Random.Range(0, offsets.Length)];
                Vector3 newRoomPos = lastPos + offset;
                Room eventPrefab = eventRoomPrefabs[Random.Range(0, eventRoomPrefabs.Count)];
                Room newRoom = Instantiate(eventPrefab, newRoomPos, Quaternion.identity, roomContainer);
                
                if (IsRoomOverlapping(newRoomPos))
                {
                    Destroy(newRoom.gameObject);
                    eventAttempts++;
                    if (eventAttempts > maxAttempts)
                    {
                        if (rooms.Count > 0)
                            lastPos = rooms[Random.Range(0, rooms.Count)].transform.position;
                        else
                            lastPos = transform.position;
                        eventAttempts = 0;
                    }
                    yield return null;
                    continue;
                }
                
                eventAttempts = 0;
                newRoom.isEventRoom = true;
                newRoom.gameObject.name = $"Room_{placedRooms}_Event";
                rooms.Add(newRoom);
                roomGrid[WorldToGridPosition(newRoomPos)] = newRoom;
                
                // Deferred door connection to the end
                // newRoom.AssignAllNeighbours(offsets, roomSpacing);
                
                lastPos = newRoomPos;
                placedRooms++;
                eventRoomsSpawned++;
                yield return null;
            }
        }
        
        Debug.Log($"[RoomGenerator] Generated {placedRooms} rooms ({normalRoomsToGenerate} normal + 1 boss) with {eventRoomsSpawned} event rooms");
        generatingRoom = false;
    }

    // GenerateDoors: reconciliation pass to ensure all doors are generated
    private void GenerateDoors()
    {
        Debug.Log("[RoomGenerator] Final door reconciliation pass");
        generatorRoom.AssignAllNeighbours(this);

        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].AssignAllNeighbours(this);
        }
    }

    // GetGenerationProgress: returns 0.0 to 1.0 based on how many rooms have been generated
    public float GetGenerationProgress()
    {
        if (rooms == null) return 0f;
        if (!generatingRoom && generatorRoom != null && generatorRoom.generationComplete) return 1f;
        
        // Rough estimate of total rooms to be spawned
        float expectedRooms = normalRoomsToGenerate + 1f;
        return Mathf.Clamp01((float)rooms.Count / expectedRooms);
    }

    // WorldToGridPosition: maps a physical world position to our mathematical Virtual Grid
    public Vector2Int WorldToGridPosition(Vector3 pos)
    {
        Vector3 offset = pos - transform.position;
        int x = Mathf.RoundToInt(offset.x / roomSpacing);
        int y = Mathf.RoundToInt(offset.z / roomSpacing);
        return new Vector2Int(x, y);
    }

    // IsRoomOverlapping: check placed room against existing rooms using O(1) grid lookup
    private bool IsRoomOverlapping(Vector3 position)
    {
        return roomGrid.ContainsKey(WorldToGridPosition(position));
    }

    // MarkFurthestRoomAsBoss: find furthest room and replace it with boss arena
    private void MarkFurthestRoomAsBoss()
    {
        // Find furthest room by physical distance since doors aren't connected yet
        Room furthestRoom = generatorRoom;
        float maxDistance = 0f;

        foreach (Room room in rooms)
        {
            if (room == generatorRoom) continue;

            float distance = Vector3.Distance(generatorRoom.transform.position, room.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestRoom = room;
            }
        }

        bossRoom = furthestRoom;
        // Replace the furthest room with boss arena
        Vector3 bossRoomPos = bossRoom.transform.position;
        Vector2Int bossGridPos = WorldToGridPosition(bossRoomPos);
        
        Destroy(bossRoom.gameObject);
        rooms.Remove(bossRoom);
        roomGrid.Remove(bossGridPos);

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
            roomGrid[bossGridPos] = bossRoom;
            
            // Deferred door connection to the end
            // bossRoom.AssignAllNeighbours(offsets, roomSpacing);
            
            Debug.Log($"[RoomGenerator] Boss arena spawned: {randomBoss.bossName}");
        }
    }


    



}
