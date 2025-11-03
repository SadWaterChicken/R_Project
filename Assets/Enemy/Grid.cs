using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

[ExecuteAlways]
public class Grid : MonoBehaviour
{
    [Header("References")]
    public Tilemap obstacleTilemap; // assign in inspector (required)

    [Header("Gizmo / Debug")]
    public bool displayGridGizmos = true;
    public bool drawShadedTiles = true;
    public bool drawTileWireframe = true;
    [Range(0f, 1f)] public float shadingAlpha = 0.5f;
    public Gradient penaltyGradient;

    [Header("Pathfinding / Terrain")]
    public TerrainType[] walkableRegions;
    public int obstacleProximityPenalty = 10;

    [Header("Agent Clearance")]
    [Tooltip("Inflate obstacles globally for all agents by this many tiles. For per-agent sizes use IsNodeAreaWalkable overloads from scripts.")]
    public int agentClearanceTiles = 0;

    // --- internal ---
    private Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    private Tilemap walkableTilemap;          // optional
    public Vector2 gridWorldSize;             // auto-calculated from tilemap bounds
    public float nodeRadius;
    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;
    private int penaltyMin = int.MaxValue;
    private int penaltyMax = int.MinValue;

    [Header("Custom Grid Size")]
    public int customGridSizeX = 10;
    public int customGridSizeY = 10;
    public bool useCustomGridSize = false;
    public bool showCoordinates = true;

    // Highlighted path tiles (set by Unit/BossUnit on path found)
    private readonly HashSet<Node> highlightedNodes = new HashSet<Node>();

    #region Unity callbacks
    private void OnValidate()
    {
        // Keep cell size in sync in editor when inspector values change
        if (obstacleTilemap != null)
            InitializeGrid();
    }

    private void Awake()
    {
        if (obstacleTilemap == null)
        {
            Debug.LogError("[Grid] obstacleTilemap not assigned on " + gameObject.name);
            return;
        }
        InitializeGrid();
    }

    private void OnEnable()
    {
        if (obstacleTilemap != null)
            InitializeGrid();
    }
    #endregion

    // Public accessor for A* maximum nodes
    public int MaxSize => gridSizeX * gridSizeY;

    // -----------------------
    // Initialization & Grid Creation
    // -----------------------
    private void InitializeGrid()
    {
        if (obstacleTilemap == null) return;

        // ⚙️ Node size = tile cell size (assumes square cells)
        nodeDiameter = obstacleTilemap.cellSize.x;
        nodeRadius = nodeDiameter / 2f;

        // ⚙️ Use tilemap bounds so the grid exactly covers the Tilemap
        BoundsInt bounds = obstacleTilemap.cellBounds;

        if (useCustomGridSize)
        {
            gridSizeX = Mathf.Max(1, customGridSizeX);
            gridSizeY = Mathf.Max(1, customGridSizeY);
        }
        else
        {
            gridSizeX = bounds.size.x;
            gridSizeY = bounds.size.y;
        }

        // ⚙️ World size based on tilemap cell count * cell size
        gridWorldSize = new Vector2(gridSizeX * nodeDiameter, gridSizeY * nodeDiameter);

        // prepare walkableRegions dictionary (if any)
        walkableRegionsDictionary.Clear();
        if (walkableRegions != null)
        {
            foreach (TerrainType region in walkableRegions)
            {
                if (region == null || region.tilemap == null) continue;
                int id = region.tilemap.GetInstanceID();
                if (!walkableRegionsDictionary.ContainsKey(id))
                    walkableRegionsDictionary.Add(id, region.terrainPenalty);
            }
        }

        CreateGrid(bounds);
    }

    private void CreateGrid(BoundsInt bounds)
    {
        penaltyMin = int.MaxValue;
        penaltyMax = int.MinValue;

        grid = new Node[gridSizeX, gridSizeY];

        // bottom-left corner (cell corner) world position
        Vector3 worldBottomLeft = obstacleTilemap.CellToWorld(bounds.min);

        if (useCustomGridSize)
        {
            worldBottomLeft = obstacleTilemap.transform.position - new Vector3(gridWorldSize.x / 2f, gridWorldSize.y / 2f, 0f);
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3Int cellPos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                Vector3 worldPoint = obstacleTilemap.GetCellCenterWorld(cellPos);

                bool walkable = obstacleTilemap.GetTile(cellPos) == null;
                int movementPenalty = walkable ? 0 : obstacleProximityPenalty;

                if (walkableRegions != null)
                {
                    foreach (var region in walkableRegions)
                    {
                        if (region == null || region.tilemap == null) continue;

                        Vector3Int rc = region.tilemap.WorldToCell(worldPoint);
                        if (region.tilemap.GetTile(rc) != null)
                        {
                            movementPenalty += region.terrainPenalty;
                            break;
                        }
                    }
                }

                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);

                if (movementPenalty < penaltyMin) penaltyMin = movementPenalty;
                if (movementPenalty > penaltyMax) penaltyMax = movementPenalty;
            }
        }

        // blur penalties for smoother values
        BlurPenaltyMap(3);
    }

    private void BlurPenaltyMap(int blurSize)
    {
        if (blurSize <= 0) return;

        int kernelSize = blurSize * 2 + 1;
        int kernelExtents = blurSize;

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        // Horizontal pass
        for (int y = 0; y < gridSizeY; y++)
        {
            int sum = 0;
            for (int ix = -kernelExtents; ix <= kernelExtents; ix++)
            {
                int sampleX = Mathf.Clamp(ix, 0, gridSizeX - 1);
                sum += grid[sampleX, y].movementPenalty;
            }
            penaltiesHorizontalPass[0, y] = sum;

            for (int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX - 1);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);
                sum = sum - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
                penaltiesHorizontalPass[x, y] = sum;
            }
        }

        // Vertical pass + apply
        penaltyMin = int.MaxValue;
        penaltyMax = int.MinValue;

        for (int x = 0; x < gridSizeX; x++)
        {
            int sum = 0;
            for (int iy = -kernelExtents; iy <= kernelExtents; iy++)
            {
                int sampleY = Mathf.Clamp(iy, 0, gridSizeY - 1);
                sum += penaltiesHorizontalPass[x, sampleY];
            }
            penaltiesVerticalPass[x, 0] = sum;

            int blurredPenalty = Mathf.RoundToInt((float)sum / (kernelSize * kernelSize));
            grid[x, 0].movementPenalty = blurredPenalty;
            penaltyMin = Mathf.Min(penaltyMin, blurredPenalty);
            penaltyMax = Mathf.Max(penaltyMax, blurredPenalty);

            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY - 1);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);
                sum = sum - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                penaltiesVerticalPass[x, y] = sum;

                blurredPenalty = Mathf.RoundToInt((float)sum / (kernelSize * kernelSize));
                grid[x, y].movementPenalty = blurredPenalty;

                penaltyMin = Mathf.Min(penaltyMin, blurredPenalty);
                penaltyMax = Mathf.Max(penaltyMax, blurredPenalty);
            }
        }
    }

    // -----------------------
    // Neighbour lookup
    // -----------------------
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    Node neighbour = grid[checkX, checkY];

                    if (!neighbour.walkable) continue; // must still skip blocked tiles

                    // Prevent cutting corners into obstacles
                    if (x != 0 && y != 0)
                    {
                        Node nodeX = grid[node.gridX + x, node.gridY];
                        Node nodeY = grid[node.gridX, node.gridY + y];

                        if (!nodeX.walkable && !nodeY.walkable)
                            continue; // cannot move diagonally if both adjacent sides are blocked
                    }

                    neighbours.Add(neighbour);
                }
            }
        }

        return neighbours;
    }

    // -----------------------
    // Convert world position -> Node
    // -----------------------
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (obstacleTilemap == null || grid == null) return null;

        BoundsInt bounds = obstacleTilemap.cellBounds;

        // Robust mapping for Tilemap-based grids (2D): use WorldToCell then map to grid array indices
        Vector3Int cell = obstacleTilemap.WorldToCell(worldPosition);

        int x = cell.x - bounds.xMin;
        int y = cell.y - bounds.yMin;

        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
            return null;

        return grid[x, y];
    }

    // Call to show the tiles along a path
    public void SetPathHighlights(Vector3[] waypoints)
    {
        highlightedNodes.Clear();
        if (waypoints == null || obstacleTilemap == null) return;

        foreach (var wp in waypoints)
        {
            var n = NodeFromWorldPoint(wp);
            if (n != null) highlightedNodes.Add(n);
        }
    }

    // Clear current highlight
    public void ClearPathHighlights()
    {
        highlightedNodes.Clear();
    }

    // -----------------------
    // Gizmo drawing (exactly aligned + shading)
    // -----------------------
    private void OnDrawGizmos()
    {
        if (obstacleTilemap == null) return;

        BoundsInt bounds = obstacleTilemap.cellBounds;
        Vector3 worldBottomLeft = obstacleTilemap.CellToWorld(bounds.min);

        float cellSize = obstacleTilemap.cellSize.x;
        float worldWidth = bounds.size.x * cellSize;
        float worldHeight = bounds.size.y * cellSize;

        gridWorldSize = new Vector2(worldWidth, worldHeight);

        // draw outline for reference
        Vector3 outlineCenter = worldBottomLeft + new Vector3(worldWidth / 2f, worldHeight / 2f, 0f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(outlineCenter, new Vector3(worldWidth, worldHeight, 0f));

        if (!displayGridGizmos) return;

        // Note: assumes you already have grid, gridSizeX, gridSizeY, penaltyMin/Max fields populated elsewhere
        if (grid == null) return;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Node n = grid[x, y];
                if (n == null) continue;

                // base tile shading
                float t = (penaltyMax != penaltyMin) ? Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty) : 0f;
                t = Mathf.Pow(t, 2f);
                Color baseColor = (penaltyGradient != null) ? penaltyGradient.Evaluate(t) : Color.Lerp(Color.white, Color.black, t);
                if (!n.walkable) baseColor = Color.red;
                baseColor.a = Mathf.Clamp01(shadingAlpha > 0f ? shadingAlpha : 0.6f);

                Gizmos.color = baseColor;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * cellSize * 0.98f);

                // overlay for path highlight
                if (highlightedNodes.Contains(n))
                {
                    var overlay = new Color(0f, 1f, 1f, 0.6f); // cyan with alpha
                    Gizmos.color = overlay;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * cellSize * 0.9f);

                    // optional: wireframe on top
                    if (drawTileWireframe)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireCube(n.worldPosition, Vector3.one * cellSize * 0.98f);
                    }
                }
            }
        }

#if UNITY_EDITOR
        // Optional coordinates label (kept as-is if you already had it)
        if (showCoordinates)
        {
            foreach (Node n in grid)
            {
                if (n == null) continue;

                float t = (penaltyMax != penaltyMin) ? Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty) : 0f;
                Color bg = (penaltyGradient != null) ? penaltyGradient.Evaluate(t) : Color.Lerp(Color.white, Color.black, t);
                if (!n.walkable) bg = Color.red;

                float luminance = bg.r * 0.299f + bg.g * 0.587f + bg.b * 0.114f;
                Color labelColor = (luminance > 0.5f) ? Color.black : Color.white;

                GUIStyle style = new GUIStyle()
                {
                    fontSize = 10,
                    normal = new GUIStyleState() { textColor = labelColor },
                    alignment = TextAnchor.MiddleCenter
                };

                UnityEditor.Handles.Label(n.worldPosition + Vector3.up * 0.1f, $"({n.gridX},{n.gridY})", style);
            }
        }
#endif
    }

    [System.Serializable]
    public class TerrainType
    {
        public Tilemap tilemap;
        public int terrainPenalty;
    }

    // Public API: Reset per-node transient A* state before a search.
    public void ResetNodeCosts()
    {
        if (grid == null) return;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Node n = grid[x, y];
                if (n == null) continue;
                n.gCost = int.MaxValue;
                n.hCost = 0;
                n.parent = null;
            }
        }
    }

    // Check if a rectangular agent footprint centered on node is fully walkable
    public bool IsNodeAreaWalkable(Node center, int halfExtentX, int halfExtentY)
    {
        if (center == null || grid == null) return false;
        int minX = center.gridX - halfExtentX;
        int maxX = center.gridX + halfExtentX;
        int minY = center.gridY - halfExtentY;
        int maxY = center.gridY + halfExtentY;

        if (minX < 0 || minY < 0 || maxX >= gridSizeX || maxY >= gridSizeY)
            return false;

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                if (!grid[x, y].walkable)
                    return false;

        return true;
    }

    // Find the closest area-walkable node to a rectangle of tiles (target footprint)
    // rect: in grid indices, inclusive width/height (RectInt uses x,y,width,height)
    public Node FindClosestClearNodeToRect(RectInt rect, Node fromNode, int agentHalfX, int agentHalfY)
    {
        if (grid == null || fromNode == null) return null;

        // Clamp rect into grid
        int rxMin = Mathf.Clamp(rect.xMin, 0, gridSizeX - 1);
        int ryMin = Mathf.Clamp(rect.yMin, 0, gridSizeY - 1);
        int rxMax = Mathf.Clamp(rect.xMax - 1, 0, gridSizeX - 1);
        int ryMax = Mathf.Clamp(rect.yMax - 1, 0, gridSizeY - 1);

        // Search ring just outside the rect first (preferred: adjacent tiles)
        int bestDist = int.MaxValue;
        Node best = null;

        System.Func<int,int,int> chebyshev = (ax, ay) =>
        {
            int dx = Mathf.Abs(ax - fromNode.gridX);
            int dy = Mathf.Abs(ay - fromNode.gridY);
            return Mathf.Max(dx, dy);
        };

        // Build an expanded rect by 1 to get perimeter
        int pxMin = Mathf.Max(rxMin - 1, 0);
        int pyMin = Mathf.Max(ryMin - 1, 0);
        int pxMax = Mathf.Min(rxMax + 1, gridSizeX - 1);
        int pyMax = Mathf.Min(ryMax + 1, gridSizeY - 1);

        // Top and bottom rows
        for (int x = pxMin; x <= pxMax; x++)
        {
            int yTop = pyMax;
            int yBot = pyMin;
            if (yTop >= 0 && yTop < gridSizeY)
            {
                Node n = grid[x, yTop];
                if (n.walkable && IsNodeAreaWalkable(n, agentHalfX, agentHalfY))
                {
                    int d = chebyshev(x, yTop);
                    if (d < bestDist) { bestDist = d; best = n; }
                }
            }
            if (yBot >= 0 && yBot < gridSizeY)
            {
                Node n = grid[x, yBot];
                if (n.walkable && IsNodeAreaWalkable(n, agentHalfX, agentHalfY))
                {
                    int d = chebyshev(x, yBot);
                    if (d < bestDist) { bestDist = d; best = n; }
                }
            }
        }
        // Left and right columns
        for (int y = pyMin + 1; y <= pyMax - 1; y++)
        {
            int xLeft = pxMin;
            int xRight = pxMax;
            if (xLeft >= 0 && xLeft < gridSizeX)
            {
                Node n = grid[xLeft, y];
                if (n.walkable && IsNodeAreaWalkable(n, agentHalfX, agentHalfY))
                {
                    int d = chebyshev(xLeft, y);
                    if (d < bestDist) { bestDist = d; best = n; }
                }
            }
            if (xRight >= 0 && xRight < gridSizeX)
            {
                Node n = grid[xRight, y];
                if (n.walkable && IsNodeAreaWalkable(n, agentHalfX, agentHalfY))
                {
                    int d = chebyshev(xRight, y);
                    if (d < bestDist) { bestDist = d; best = n; }
                }
            }
        }

        // Fallback: try inside rect if perimeter failed (for overlap cases)
        if (best == null)
        {
            for (int x = rxMin; x <= rxMax; x++)
                for (int y = ryMin; y <= ryMax; y++)
                {
                    Node n = grid[x, y];
                    if (n.walkable && IsNodeAreaWalkable(n, agentHalfX, agentHalfY))
                    {
                        int d = chebyshev(x, y);
                        if (d < bestDist) { bestDist = d; best = n; }
                    }
                }
        }

        return best;
    }
}
