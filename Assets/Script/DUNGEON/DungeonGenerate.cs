using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerate : MonoBehaviour
{
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject corridorPrefab;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(100, 100);
    [SerializeField] private int roomCount = 5;
    [SerializeField] private int maxAttempts = 500;
    [SerializeField] private int minRoomWidth = 5;
    [SerializeField] private int maxRoomWidth = 10;
    [SerializeField] private float gridCellSize = 2.5f;
    [SerializeField] private bool showVisuals = true;
    [SerializeField] private bool showMST = true;
    [SerializeField] [Range(1, 5)] private int corridorWidth = 2;

    private List<DungeonRoom> rooms = new List<DungeonRoom>();
    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> corridorCells = new HashSet<Vector2Int>();
    private List<RoomConnection> mstEdges = new List<RoomConnection>();
    private Dictionary<RoomConnection, (Vector2Int start, Vector2Int end)> corridorEndpoints = new Dictionary<RoomConnection, (Vector2Int, Vector2Int)>();
    private DungeonPathfinder2D pathfinder;
    private Transform dungeonParent;
    private int roomIdCounter = 0;

    [SerializeField] private GameObject playerSpawnPrefab;
    private Vector3 playerSpawnPosition;

    private void Start()
    {
        // Don't auto-generate - let DungeonManager control this
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        ValidateReferences();
        Clear();
        
        PlaceRooms();
        if (rooms.Count >= 2)
        {
            Triangulate();
            InitializePathfinder();
            VisualizeConnections();
            PathfindHallways();
            CreateWalls();
        }
        
        // Set spawn position at first room center
        if (rooms.Count > 0)
            playerSpawnPosition = rooms[0].position;
    }

    private void ValidateReferences()
    {
        if (floorPrefab == null) Debug.LogError("Floor prefab not assigned!");
        if (corridorPrefab == null) Debug.LogError("Corridor prefab not assigned!");
    }

    private void Clear()
    {
        if (dungeonParent != null) DestroyImmediate(dungeonParent.gameObject);
        
        rooms.Clear();
        occupiedCells.Clear();
        corridorCells.Clear();
        mstEdges.Clear();
        corridorEndpoints.Clear();
        roomIdCounter = 0;
        
        dungeonParent = new GameObject("Dungeon").transform;
    }

    private void Triangulate()
    {
        mstEdges = DungeonGraphAlgorithms.GenerateDungeonConnections(rooms);
    }

    private void VisualizeConnections()
    {
        if (!showVisuals || !showMST) return;

        GameObject visParent = new GameObject("Visualizations");
        visParent.transform.SetParent(dungeonParent);

        foreach (var room in rooms)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.name = $"RoomPoint_{room.id}";
            point.transform.position = room.position;
            point.transform.localScale = Vector3.one * 2f;
            point.transform.SetParent(visParent.transform);
            DestroyImmediate(point.GetComponent<Collider>());
        }

        foreach (var edge in mstEdges)
        {
            GameObject line = new GameObject($"Edge_{edge.roomA.id}_{edge.roomB.id}");
            line.transform.SetParent(visParent.transform);
            
            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, edge.roomA.position);
            lr.SetPosition(1, edge.roomB.position);
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.green;
            lr.endColor = Color.green;
            lr.startWidth = 0.5f;
            lr.endWidth = 0.5f;
        }
    }

    private void PlaceRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            DungeonRoom room = TryPlaceRoom();
            if (room != null)
            {
                rooms.Add(room);
                CreateRoomFloors(room);
            }
        }
        Debug.Log($"✓ Placed {rooms.Count} rooms");
    }

    private DungeonRoom TryPlaceRoom()
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2Int roomSize = RandomRoomSize();
            Vector2Int gridPos = RandomGridPosition();

            if (IsRoomOccupied(gridPos, roomSize))
                continue;

            DungeonRoom room = CreateRoom(gridPos, roomSize);
            if (!RoomOverlaps(room))
            {
                OccupyCells(gridPos, roomSize);
                return room;
            }
        }
        return null;
    }

    private Vector2Int RandomRoomSize()
    {
        int width = Random.Range(minRoomWidth, maxRoomWidth + 1);
        int depth = Random.Range(minRoomWidth, maxRoomWidth + 1);
        if (width % 2 == 0) width++;
        if (depth % 2 == 0) depth++;
        return new Vector2Int(Mathf.Min(width, maxRoomWidth), Mathf.Min(depth, maxRoomWidth));
    }

    private Vector2Int RandomGridPosition() => new Vector2Int(Random.Range(-50, 51), Random.Range(-50, 51));

    private DungeonRoom CreateRoom(Vector2Int gridPos, Vector2Int size)
    {
        float worldX = gridPos.x * gridCellSize + (size.x * gridCellSize) * 0.5f;
        float worldZ = gridPos.y * gridCellSize + (size.y * gridCellSize) * 0.5f;
        
        DungeonRoom room = new DungeonRoom(
            new Vector3(worldX, 0f, worldZ),
            new Vector3Int(size.x, 1, size.y),
            roomIdCounter++
        );
        room.gridStartX = gridPos.x;
        room.gridStartZ = gridPos.y;
        return room;
    }

    private bool IsRoomOccupied(Vector2Int pos, Vector2Int size)
    {
        const int Spacing = 3;
        for (int x = pos.x - Spacing; x < pos.x + size.x + Spacing; x++)
        {
            for (int z = pos.y - Spacing; z < pos.y + size.y + Spacing; z++)
            {
                if (occupiedCells.Contains(new Vector2Int(x, z)))
                    return true;
            }
        }
        return false;
    }

    private bool RoomOverlaps(DungeonRoom candidate)
    {
        foreach (var room in rooms)
        {
            if (candidate.OverlapsWith(room))
                return true;
        }
        return false;
    }

    private void OccupyCells(Vector2Int pos, Vector2Int size)
    {
        for (int x = pos.x; x < pos.x + size.x; x++)
        {
            for (int z = pos.y; z < pos.y + size.y; z++)
            {
                occupiedCells.Add(new Vector2Int(x, z));
            }
        }
    }



    private void CreateRoomFloors(DungeonRoom room)
    {
        GameObject roomObj = new GameObject($"Room_{room.id}");
        roomObj.transform.SetParent(dungeonParent);
        room.roomObject = roomObj;

        GameObject floorsParent = new GameObject("Floors");
        floorsParent.transform.SetParent(roomObj.transform);

        for (int x = 0; x < room.size.x; x++)
        {
            for (int z = 0; z < room.size.z; z++)
            {
                int gridX = room.gridStartX + x;
                int gridZ = room.gridStartZ + z;
                Vector3 tilePos = GridToWorldPos(gridX, gridZ);

                GameObject floor = Instantiate(floorPrefab, tilePos, Quaternion.identity, floorsParent.transform);
                floor.name = $"Floor_{gridX}_{gridZ}";
                room.tiles.Add(floor);
            }
        }
    }

    private Vector3 GridToWorldPos(int gridX, int gridZ)
    {
        return new Vector3(
            gridX * gridCellSize + gridCellSize * 0.5f,
            0f,
            gridZ * gridCellSize + gridCellSize * 0.5f
        );
    }

    private void CreateWalls()
    {
        // Build exact door positions from corridor endpoints (1 cell per hallway end)
        var doorPositions = new HashSet<Vector2Int>();
        foreach (var kv in corridorEndpoints)
        {
            doorPositions.Add(kv.Value.start);
            doorPositions.Add(kv.Value.end);
        }

        CreateRoomWalls(doorPositions);
        CreateCorridorWalls();
    }

    private void CreateRoomWalls(HashSet<Vector2Int> doorPositions)
    {
        if (wallPrefab == null || doorPrefab == null) return;

        GameObject doorsParent = new GameObject("Doors");
        doorsParent.transform.SetParent(dungeonParent);

        foreach (var room in rooms)
        {
            GameObject roomObj = room.roomObject ?? new GameObject($"Room_{room.id}");
            GameObject wallsParent = new GameObject("Walls");
            wallsParent.transform.SetParent(roomObj.transform);

            // Top and Bottom walls
            for (int x = 0; x < room.size.x; x++)
            {
                int gridX = room.gridStartX + x;

                // Top wall row (z = gridStartZ - 1)
                var topCell = new Vector2Int(gridX, room.gridStartZ - 1);
                if (doorPositions.Contains(topCell))
                    PlaceDoor(topCell, doorsParent, $"door_{room.id}_top_{gridX}");
                else
                    CreateWall(wallsParent, gridX, room.gridStartZ - 1, "WallTop");

                // Bottom wall row (z = gridStartZ + size.z)
                var botCell = new Vector2Int(gridX, room.gridStartZ + room.size.z);
                if (doorPositions.Contains(botCell))
                    PlaceDoor(botCell, doorsParent, $"door_{room.id}_bot_{gridX}");
                else
                    CreateWall(wallsParent, gridX, room.gridStartZ + room.size.z, "WallBottom");
            }

            // Left and Right walls
            for (int z = 0; z < room.size.z; z++)
            {
                int gridZ = room.gridStartZ + z;

                var leftCell = new Vector2Int(room.gridStartX - 1, gridZ);
                if (doorPositions.Contains(leftCell))
                    PlaceDoor(leftCell, doorsParent, $"door_{room.id}_left_{gridZ}");
                else
                    CreateWall(wallsParent, room.gridStartX - 1, gridZ, "WallLeft");

                var rightCell = new Vector2Int(room.gridStartX + room.size.x, gridZ);
                if (doorPositions.Contains(rightCell))
                    PlaceDoor(rightCell, doorsParent, $"door_{room.id}_right_{gridZ}");
                else
                    CreateWall(wallsParent, room.gridStartX + room.size.x, gridZ, "WallRight");
            }
        }
    }

    private void CreateWall(GameObject wallsParent, int gridX, int gridZ, string wallType)
    {
        if (wallPrefab == null) return;

        float wallX, wallZ;

        if (wallType == "WallLeft")
        {
            wallX = gridX * gridCellSize + gridCellSize;
            wallZ = gridZ * gridCellSize + gridCellSize * 0.5f;
        }
        else if (wallType == "WallRight")
        {
            wallX = gridX * gridCellSize;
            wallZ = gridZ * gridCellSize + gridCellSize * 0.5f;
        }
        else if (wallType == "WallTop")
        {
            wallX = gridX * gridCellSize + gridCellSize * 0.5f;
            wallZ = gridZ * gridCellSize + gridCellSize;
        }
        else // WallBottom
        {
            wallX = gridX * gridCellSize + gridCellSize * 0.5f;
            wallZ = gridZ * gridCellSize;
        }

        Vector3 wallPos = new Vector3(wallX, 0f, wallZ);
        Quaternion wallRot = (wallType == "WallLeft" || wallType == "WallRight") 
            ? Quaternion.Euler(0, 90, 0) 
            : Quaternion.identity;

        GameObject wall = Instantiate(wallPrefab, wallPos, wallRot, wallsParent.transform);
        wall.name = $"Wall_{wallType}_{gridX}_{gridZ}";
    }

    private void CreateCorridorWalls()
    {
        if (wallPrefab == null) return;

        GameObject corridorParent = new GameObject("Corridors");
        corridorParent.transform.SetParent(dungeonParent);

        GameObject wallsParent = new GameObject("CorridorWalls");
        wallsParent.transform.SetParent(corridorParent.transform);

        HashSet<(Vector2Int, string)> placed = new HashSet<(Vector2Int, string)>();

        foreach (var cell in corridorCells)
        {
            TryPlaceCorridorWall(cell, Vector2Int.up,    "Top",    wallsParent, placed);
            TryPlaceCorridorWall(cell, Vector2Int.down,  "Bottom", wallsParent, placed);
            TryPlaceCorridorWall(cell, Vector2Int.left,  "Left",   wallsParent, placed);
            TryPlaceCorridorWall(cell, Vector2Int.right, "Right",  wallsParent, placed);
        }
    }

    private void TryPlaceCorridorWall(Vector2Int cell, Vector2Int dir, string side,
        GameObject parent, HashSet<(Vector2Int, string)> placed)
    {
        Vector2Int neighbor = cell + dir;
        // Skip if neighbor is another corridor cell or already handled
        if (corridorCells.Contains(neighbor)) return;
        if (placed.Contains((cell, side))) return;
        placed.Add((cell, side));

        // Don't place corridor wall where a room wall / door is (room walls handle that side)
        if (occupiedCells.Contains(neighbor)) return;

        CreateCorridorWall(parent, cell.x, cell.y, side);
    }

    private void CreateCorridorWall(GameObject wallsParent, int gridX, int gridZ, string wallType)
    {
        if (wallPrefab == null) return;

        float wallX, wallZ;

        if (wallType == "Left")
        {
            wallX = gridX * gridCellSize;
            wallZ = gridZ * gridCellSize + gridCellSize * 0.5f;
        }
        else if (wallType == "Right")
        {
            wallX = gridX * gridCellSize + gridCellSize;
            wallZ = gridZ * gridCellSize + gridCellSize * 0.5f;
        }
        else if (wallType == "Top")
        {
            wallX = gridX * gridCellSize + gridCellSize * 0.5f;
            wallZ = gridZ * gridCellSize + gridCellSize;
        }
        else // Bottom
        {
            wallX = gridX * gridCellSize + gridCellSize * 0.5f;
            wallZ = gridZ * gridCellSize;
        }

        Vector3 wallPos = new Vector3(wallX, 0f, wallZ);
        Quaternion wallRot = (wallType == "Left" || wallType == "Right") 
            ? Quaternion.Euler(0, 90, 0) 
            : Quaternion.identity;

        GameObject wall = Instantiate(wallPrefab, wallPos, wallRot, wallsParent.transform);
        wall.name = $"CorridorWall_{wallType}_{gridX}_{gridZ}";
    }

    private void PlaceHallwayFloors()
    {
        if (corridorPrefab == null)
        {
            Debug.LogWarning("Corridor prefab not assigned!");
            return;
        }

        GameObject hallwaysParent = new GameObject("Hallways");
        hallwaysParent.transform.SetParent(dungeonParent);

        foreach (var cell in corridorCells)
        {
            Vector3 pos = GridToWorldPos(cell.x, cell.y);
            GameObject hallway = Instantiate(corridorPrefab, pos, Quaternion.identity, hallwaysParent.transform);
            hallway.name = $"Hallway_{cell.x}_{cell.y}";
        }
    }

    private void PathfindHallways()
    {
        corridorEndpoints.Clear();
        
        foreach (var edge in mstEdges)
        {
            var pathA = GetRoomBoundaryCell(edge.roomA, edge.roomB);
            var pathB = GetRoomBoundaryCell(edge.roomB, edge.roomA);

            List<Vector2Int> path = null;
            try
            {
                path = pathfinder.FindPath(pathA, pathB, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                    var pathCost = new DungeonPathfinder2D.PathCost();

                    if (occupiedCells.Contains(b.Position))
                    {
                        pathCost.traversable = false;
                        return pathCost;
                    }

                    pathCost.traversable = true;
                    pathCost.cost = Vector2Int.Distance(b.Position, pathB);

                    // Heavy turn penalty to prevent stair/zigzag pattern
                    if (a.Previous != null)
                    {
                        Vector2Int prevDir = a.Position - a.Previous.Position;
                        Vector2Int curDir  = b.Position - a.Position;
                        if (prevDir != curDir)
                            pathCost.cost += 15f;
                    }

                    pathCost.cost += corridorCells.Contains(b.Position) ? 0.5f : 1f;

                    return pathCost;
                });
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"A* failed: {ex.Message}, using L-shaped fallback");
            }

            if (path != null && path.Count > 0)
                AddPathToCorridor(path, edge);
            else
                CreateLShapedHallway(edge, pathA, pathB);
        }
        
        PlaceHallwayFloors();
    }

    private Vector2Int GetRoomBoundaryCell(DungeonRoom room, DungeonRoom targetRoom)
    {
        Vector2Int roomCenter = new Vector2Int((int)(room.position.x / gridCellSize), (int)(room.position.z / gridCellSize));
        Vector2Int targetCenter = new Vector2Int((int)(targetRoom.position.x / gridCellSize), (int)(targetRoom.position.z / gridCellSize));
        
        Vector2Int diff = targetCenter - roomCenter;
        
        // Find nearest room boundary cell that faces the target
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            // Target is more left/right
            if (diff.x > 0)
                return new Vector2Int(room.gridStartX + room.size.x, roomCenter.y);
            else
                return new Vector2Int(room.gridStartX - 1, roomCenter.y);
        }
        else
        {
            // Target is more up/down
            if (diff.y > 0)
                return new Vector2Int(roomCenter.x, room.gridStartZ + room.size.z);
            else
                return new Vector2Int(roomCenter.x, room.gridStartZ - 1);
        }
    }

    private Vector2Int RoomToGridPos(DungeonRoom room) => 
        new Vector2Int((int)(room.position.x / gridCellSize), (int)(room.position.z / gridCellSize));

    private void AddPathToCorridor(List<Vector2Int> path, RoomConnection edge)
    {
        // Widen perpendicular to each path segment direction
        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int cell = path[i];

            // Determine travel direction at this cell
            Vector2Int dir = Vector2Int.zero;
            if (i + 1 < path.Count) dir = path[i + 1] - cell;
            else if (i > 0)         dir = cell - path[i - 1];

            // Perpendicular to dir
            Vector2Int perp = new Vector2Int(-dir.y, dir.x);
            int half = corridorWidth / 2;

            for (int w = -half; w < corridorWidth - half; w++)
            {
                corridorCells.Add(cell + perp * w);
            }
        }

        corridorEndpoints[edge] = (path[0], path[path.Count - 1]);
    }

    private void CreateCorridor(RoomConnection edge)
    {
        var pathA = GetRoomBoundaryCell(edge.roomA, edge.roomB);
        var pathB = GetRoomBoundaryCell(edge.roomB, edge.roomA);
        CreateLShapedHallway(edge, pathA, pathB);
    }

    private void CreateLShapedHallway(RoomConnection edge, Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> centerPath = new List<Vector2Int>();
        int half = corridorWidth / 2;

        // Horizontal segment first
        int x = from.x;
        int xDir = (to.x > from.x) ? 1 : -1;
        while (x != to.x)
        {
            centerPath.Add(new Vector2Int(x, from.y));
            x += xDir;
        }

        // Vertical segment
        int z = from.y;
        int zDir = (to.y > from.y) ? 1 : -1;
        while (z != to.y + zDir)
        {
            centerPath.Add(new Vector2Int(to.x, z));
            z += zDir;
        }

        if (centerPath.Count == 0) return;

        // Expand each center cell perpendicular to direction
        for (int i = 0; i < centerPath.Count; i++)
        {
            Vector2Int cell = centerPath[i];
            Vector2Int dir = Vector2Int.right;
            if (i + 1 < centerPath.Count) dir = centerPath[i + 1] - cell;
            else if (i > 0)               dir = cell - centerPath[i - 1];
            Vector2Int perp = new Vector2Int(-dir.y, dir.x);

            for (int w = -half; w < corridorWidth - half; w++)
                corridorCells.Add(cell + perp * w);
        }

        corridorEndpoints[edge] = (centerPath[0], centerPath[centerPath.Count - 1]);
    }

    private void InitializePathfinder()
    {
        int minX = int.MaxValue, minZ = int.MaxValue;
        int maxX = int.MinValue, maxZ = int.MinValue;

        foreach (var room in rooms)
        {
            int gridX = (int)(room.position.x / gridCellSize);
            int gridZ = (int)(room.position.z / gridCellSize);
            minX = Mathf.Min(minX, gridX - room.size.x / 2);
            minZ = Mathf.Min(minZ, gridZ - room.size.z / 2);
            maxX = Mathf.Max(maxX, gridX + room.size.x / 2);
            maxZ = Mathf.Max(maxZ, gridZ + room.size.z / 2);
        }

        const int Padding = 5;
        minX -= Padding;
        minZ -= Padding;
        maxX += Padding;
        maxZ += Padding;

        int width = maxX - minX;
        int height = maxZ - minZ;
        Vector2Int gridOffset = new Vector2Int(minX, minZ);

        pathfinder = new DungeonPathfinder2D(new Vector2Int(width, height));
        pathfinder.GridOffset = gridOffset;
    }

    private void PlaceDoor(Vector2Int gridPos, GameObject parent, string name)
    {
        Vector3 pos = GridToWorldPos(gridPos.x, gridPos.y);
        GameObject door = Instantiate(doorPrefab, pos, Quaternion.identity, parent.transform);
        door.name = name;
    }

    public List<DungeonRoom> GetRooms() => rooms;
    public List<RoomConnection> GetMSTEdges() => mstEdges;
    public Vector3 GetPlayerSpawnPosition() => playerSpawnPosition;

    private void OnDrawGizmos()
    {
        if (!showVisuals) return;

        Gizmos.color = Color.cyan;
        foreach (var room in rooms)
        {
            Gizmos.DrawWireCube(room.position, Vector3.one * 5f);
        }

        if (showMST)
        {
            Gizmos.color = Color.green;
            foreach (var edge in mstEdges)
            {
                Gizmos.DrawLine(edge.roomA.position, edge.roomB.position);
            }
        }
    }
}
