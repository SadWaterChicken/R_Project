using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Dungeon generator using node-based approach with Gabriel Graph and MST
/// 
/// Process:
/// 1. Generate random node positions
/// 2. Apply Gabriel Graph to create potential connections
/// 3. Apply Minimal Spanning Tree to remove cycles
/// 4. Optionally add random loops for variety
/// 5. Determine room type for each node based on connections
/// 6. Spawn actual room prefabs at node positions
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomPrefabManager roomPrefabManager;
    [SerializeField] private GameObject floorTilePrefab;
    [SerializeField] private GameObject wallTilePrefab;

    [Header("Generation Settings")]
    [SerializeField] private int targetRoomCount = 15;
    [SerializeField] private float roomSpacing = 12f;
    [SerializeField] private float floorTileSize = 11f; // Size of your floor tile (11x11)
    [SerializeField] private int minRoomDistance = 1;
    [SerializeField] private int maxRoomDistance = 3;
    [SerializeField] private float generationSpeed = 0.1f;
    [SerializeField] private int randomLoops = 2;
    [SerializeField] private Vector2 dungeonAreaSize = new Vector2(100f, 100f);

    [Header("Visualization")]
    [SerializeField] private bool visualizeNodes = true;
    [SerializeField] private bool visualizeConnections = true;

    private List<DungeonNode> nodes = new List<DungeonNode>();
    private List<DungeonRoomInstance> spawnedRooms = new List<DungeonRoomInstance>();
    private List<GameObject> corridorTiles = new List<GameObject>();
    private bool isGenerating = false;

    private void Start()
    {
        ValidateManager();
        StartDungeonGeneration();
    }

    private void ValidateManager()
    {
        if (roomPrefabManager == null)
        {
            roomPrefabManager = GetComponent<RoomPrefabManager>();
            if (roomPrefabManager == null)
            {
                Debug.LogError("RoomPrefabManager not found!");
            }
        }
    }

    public void StartDungeonGeneration()
    {
        if (isGenerating)
        {
            Debug.LogWarning("Dungeon generation already in progress!");
            return;
        }

        ClearDungeon();
        isGenerating = true;
        StartCoroutine(GenerateDungeon());
    }

    private void ClearDungeon()
    {
        foreach (var room in spawnedRooms)
        {
            if (room != null)
                Destroy(room.gameObject);
        }
        
        foreach (var tile in corridorTiles)
        {
            if (tile != null)
                Destroy(tile);
        }
        corridorTiles.Clear();
        
        spawnedRooms.Clear();
        nodes.Clear();
    }

    private IEnumerator GenerateDungeon()
    {
        Debug.Log("=== Starting Node-Based Dungeon Generation ===");

        // Step 1: Generate random node positions
        Debug.Log($"Step 1: Generating {targetRoomCount} nodes...");
        GenerateNodes();
        yield return new WaitForSeconds(generationSpeed);

        // Step 2: Apply Relative Neighborhood Graph
        Debug.Log("Step 2: Creating Relative Neighborhood Graph...");
        GraphAlgorithms.CreateRelativeNeighborhoodGraph(nodes);
        yield return new WaitForSeconds(generationSpeed);

        // Step 3: Apply Minimal Spanning Tree
        Debug.Log("Step 3: Applying Minimal Spanning Tree...");
        GraphAlgorithms.ApplyMinimalSpanningTree(nodes);
        yield return new WaitForSeconds(generationSpeed);

        // Step 3.5: Validate all nodes are connected
        Debug.Log("Step 3.5: Validating node connectivity...");
        if (!ValidateNodeConnectivity())
        {
            Debug.LogError("Failed to create connected dungeon! Some nodes are unreachable.");
            isGenerating = false;
            yield break;
        }
        yield return new WaitForSeconds(generationSpeed);

        // Step 4: Determine room types based on connections
        Debug.Log("Step 4: Determining room types for each node...");
        foreach (var node in nodes)
        {
            node.DeterminedRoomType = node.DetermineRoomType();
        }
        yield return new WaitForSeconds(generationSpeed);
// Step 6: Create corridors for long connections
        Debug.Log("Step 6: Creating corridors for distant connections...");
        CreateCorridors();
        yield return new WaitForSeconds(generationSpeed);

        
        // Step 5: Spawn actual rooms
        Debug.Log("Step 5: Spawning rooms...");
        foreach (var node in nodes)
        {
            SpawnRoomAtNode(node);
            yield return new WaitForSeconds(generationSpeed);
        }

        isGenerating = false;
        Debug.Log($"=== Dungeon Generation Complete! {spawnedRooms.Count} rooms spawned ===");
        PrintDungeonInfo();
    }

    /// <summary>
    /// Generate random node positions using connected growth approach
    /// Ensures all nodes can be connected with horizontal/vertical edges
    /// </summary>
    private void GenerateNodes()
    {
        nodes.Clear();

        // Start with one node at origin
        nodes.Add(new DungeonNode(Vector3.zero));

        // Generate remaining nodes using connected growth
        // Each new node is placed adjacent to an existing node with random distance
        for (int i = 1; i < targetRoomCount; i++)
        {
            int attempts = 0;
            bool placed = false;

            while (!placed && attempts < 50)
            {
                // Pick a random existing node
                DungeonNode baseNode = nodes[Random.Range(0, nodes.Count)];

                // Try to place new node in one of 4 directions with random distance
                Direction[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
                Direction randomDir = directions[Random.Range(0, directions.Length)];
                
                // Random distance between min and max
                int distance = Random.Range(minRoomDistance, maxRoomDistance + 1);

                Vector3 newPosition = GetAdjacentPosition(baseNode.Position, randomDir, distance);

                // Check if position is already occupied
                bool occupied = false;
                foreach (var node in nodes)
                {
                    if (Vector3.Distance(node.Position, newPosition) < 1f)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied)
                {
                    nodes.Add(new DungeonNode(newPosition));
                    placed = true;
                }

                attempts++;
            }

            // Fallback: if we can't place adjacently, try random position on grid
            if (!placed)
            {
                for (int fallback = 0; fallback < 100; fallback++)
                {
                    Vector3 position = new Vector3(
                        Random.Range(-5, 6) * roomSpacing,
                        0,
                        Random.Range(-5, 6) * roomSpacing
                    );

                    bool occupied = false;
                    foreach (var node in nodes)
                    {
                        if (Vector3.Distance(node.Position, position) < roomSpacing * 0.9f)
                        {
                            occupied = true;
                            break;
                        }
                    }

                    if (!occupied)
                    {
                        nodes.Add(new DungeonNode(position));
                        break;
                    }
                }
            }
        }

        Debug.Log($"Generated {nodes.Count} nodes");
    }

    private Vector3 GetAdjacentPosition(Vector3 position, Direction direction, int distance = 1)
    {
        float offset = roomSpacing * distance;
        return direction switch
        {
            Direction.Up => position + new Vector3(0, 0, offset),
            Direction.Down => position + new Vector3(0, 0, -offset),
            Direction.Left => position + new Vector3(-offset, 0, 0),
            Direction.Right => position + new Vector3(offset, 0, 0),
            _ => position
        };
    }

    /// <summary>
    /// Spawn a room prefab at a node's position
    /// </summary>
    private void SpawnRoomAtNode(DungeonNode node)
    {
        GameObject prefab = roomPrefabManager.GetRandomRoom(node.DeterminedRoomType);
        
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab for room type {node.DeterminedRoomType}, skipping...");
            return;
        }

        GameObject roomObject = Instantiate(prefab, node.Position, Quaternion.identity, transform);
        roomObject.name = $"Room_{spawnedRooms.Count}_{node.DeterminedRoomType}";

        DungeonRoomInstance roomInstance = roomObject.GetComponent<DungeonRoomInstance>();
        if (roomInstance == null)
        {
            roomInstance = roomObject.AddComponent<DungeonRoomInstance>();
        }

        Vector3Int roomSize = roomPrefabManager.GetRoomSize(node.DeterminedRoomType);
        roomInstance.Initialize(node.DeterminedRoomType, roomSize, this);

        spawnedRooms.Add(roomInstance);
        node.IsSpawned = true;
    }

    /// <summary>
    /// Create corridors between rooms that are more than 2 units apart
    /// Fill the connection path with floor tiles
    /// </summary>
    private void CreateCorridors()
    {
        if (floorTilePrefab == null)
        {
            Debug.LogWarning("FloorTile prefab not assigned, skipping corridor creation");
            return;
        }

        HashSet<(DungeonNode, DungeonNode)> processedConnections = new HashSet<(DungeonNode, DungeonNode)>();
        int corridorsCreated = 0;

        foreach (var node in nodes)
        {
            foreach (var connection in node.Connections)
            {
                var pair1 = (node, connection);
                var pair2 = (connection, node);

                if (processedConnections.Contains(pair1) || processedConnections.Contains(pair2))
                    continue;

                processedConnections.Add(pair1);

                // Create corridor for all connections (fill the path between rooms)
                CreateCorridorBetween(node.Position, connection.Position);
                corridorsCreated++;
            }
        }

        Debug.Log($"Created {corridorsCreated} corridors");
    }

    private void CreateCorridorBetween(Vector3 start, Vector3 end)
    {
        // Determine if corridor is horizontal or vertical
        float deltaX = Mathf.Abs(end.x - start.x);
        float deltaZ = Mathf.Abs(end.z - start.z);

        if (deltaX > deltaZ)
        {
            // Horizontal corridor
            CreateHorizontalCorridor(start, end);
        }
        else
        {
            // Vertical corridor
            CreateVerticalCorridor(start, end);
        }
    }

    private void CreateHorizontalCorridor(Vector3 start, Vector3 end)
    {
        float startX = Mathf.Min(start.x, end.x);
        float endX = Mathf.Max(start.x, end.x);
        float z = start.z;

        // Spawn floor tiles at intervals matching tile size to avoid gaps
        for (float x = startX + floorTileSize; x < endX; x += floorTileSize)
        {
            Vector3 floorPos = new Vector3(x, 0, z);
            SpawnCorridorTile(floorTilePrefab, floorPos, "Floor");
        }
    }

    private void CreateVerticalCorridor(Vector3 start, Vector3 end)
    {
        float startZ = Mathf.Min(start.z, end.z);
        float endZ = Mathf.Max(start.z, end.z);
        float x = start.x;

        // Spawn floor tiles at intervals matching tile size to avoid gaps
        for (float z = startZ + floorTileSize; z < endZ; z += floorTileSize)
        {
            Vector3 floorPos = new Vector3(x, 0, z);
            SpawnCorridorTile(floorTilePrefab, floorPos, "Floor");
        }
    }

    private void SpawnCorridorTile(GameObject prefab, Vector3 position, string type)
    {
        GameObject tile = Instantiate(prefab, position, Quaternion.identity, transform);
        tile.name = $"Corridor_{type}_{corridorTiles.Count}";
        corridorTiles.Add(tile);
    }

    /// <summary>
    /// Validate that all nodes are connected and reachable from the starting node
    /// </summary>
    private bool ValidateNodeConnectivity()
    {
        if (nodes.Count == 0)
            return false;

        // Use BFS to check if all nodes are reachable from first node
        HashSet<DungeonNode> visited = new HashSet<DungeonNode>();
        Queue<DungeonNode> queue = new Queue<DungeonNode>();
        
        queue.Enqueue(nodes[0]);
        visited.Add(nodes[0]);

        while (queue.Count > 0)
        {
            DungeonNode current = queue.Dequeue();

            foreach (var connection in current.Connections)
            {
                if (!visited.Contains(connection))
                {
                    visited.Add(connection);
                    queue.Enqueue(connection);
                }
            }
        }

        bool allConnected = visited.Count == nodes.Count;
        
        if (!allConnected)
        {
            Debug.LogWarning($"Only {visited.Count}/{nodes.Count} nodes are reachable!");
            
            // List unreachable nodes
            foreach (var node in nodes)
            {
                if (!visited.Contains(node))
                {
                    Debug.LogWarning($"Unreachable node at {node.Position}");
                }
            }
        }
        else
        {
            Debug.Log($"All {nodes.Count} nodes are connected!");
        }

        // Validate connections are aligned
        int invalidConnections = 0;
        foreach (var node in nodes)
        {
            foreach (var connection in node.Connections)
            {
                float deltaX = Mathf.Abs(node.Position.x - connection.Position.x);
                float deltaZ = Mathf.Abs(node.Position.z - connection.Position.z);
                
                if (deltaX > 0.1f && deltaZ > 0.1f)
                {
                    Debug.LogWarning($"Invalid diagonal connection: {node.Position} -> {connection.Position}");
                    invalidConnections++;
                }
            }
        }

        if (invalidConnections > 0)
        {
            Debug.LogWarning($"Found {invalidConnections / 2} diagonal connections!");
        }

        return allConnected;
    }

    public RoomPrefabManager GetPrefabManager() => roomPrefabManager;

    private void PrintDungeonInfo()
    {
        Debug.Log("=== Dungeon Info ===");
        Debug.Log($"Total Rooms: {spawnedRooms.Count}");
        Debug.Log($"Total Nodes: {nodes.Count}");
        
        Dictionary<RoomPrefabManager.RoomType, int> roomTypeCounts = new Dictionary<RoomPrefabManager.RoomType, int>();
        foreach (var room in spawnedRooms)
        {
            var type = room.GetRoomType();
            if (!roomTypeCounts.ContainsKey(type))
                roomTypeCounts[type] = 0;
            roomTypeCounts[type]++;
        }

        Debug.Log("=== Room Types ===");
        foreach (var kvp in roomTypeCounts.OrderByDescending(x => x.Value))
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }

        int totalConnections = nodes.Sum(n => n.Connections.Count) / 2;
        Debug.Log($"Total Connections: {totalConnections}");
    }

    private void OnDrawGizmos()
    {
        if (nodes == null || nodes.Count == 0)
            return;

        // Draw nodes
        if (visualizeNodes)
        {
            Gizmos.color = Color.yellow;
            foreach (var node in nodes)
            {
                Gizmos.DrawSphere(node.Position, 0.5f);
            }
        }

        // Draw connections
        if (visualizeConnections)
        {
            Gizmos.color = Color.cyan;
            HashSet<(DungeonNode, DungeonNode)> drawnConnections = new HashSet<(DungeonNode, DungeonNode)>();
            
            foreach (var node in nodes)
            {
                foreach (var connection in node.Connections)
                {
                    var pair1 = (node, connection);
                    var pair2 = (connection, node);
                    
                    if (!drawnConnections.Contains(pair1) && !drawnConnections.Contains(pair2))
                    {
                        Gizmos.DrawLine(node.Position + Vector3.up * 0.1f, connection.Position + Vector3.up * 0.1f);
                        drawnConnections.Add(pair1);
                    }
                }
            }
        }
    }
}
