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
    public bool drawShadedTiles = true;       // toggle the filled shading
    public bool drawTileWireframe = true;     // toggle the exact wireframe
    [Range(0f, 1f)] public float shadingAlpha = 0.5f;
    public Gradient penaltyGradient;          // map penalty -> color

    [Header("Pathfinding / Terrain")]
    public TerrainType[] walkableRegions;     // optional extra tilemaps for penalties
    public int obstacleProximityPenalty = 10;

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
        Vector3 worldBottomLeft = obstacleTilemap.CellToWorld(bounds.min);

        float percentX = (worldPosition.x - worldBottomLeft.x) / gridWorldSize.x;
        float percentY = (worldPosition.y - worldBottomLeft.y) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.Clamp(Mathf.RoundToInt((gridSizeX - 1) * percentX), 0, gridSizeX - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt((gridSizeY - 1) * percentY), 0, gridSizeY - 1);

        return grid[x, y];
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

        if (!displayGridGizmos || grid == null) return;

        // draw filled tiles
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Node n = grid[x, y];
                if (n == null) continue;

                float t = (penaltyMax != penaltyMin) ? Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty) : 0f;
                t = Mathf.Pow(t, 2f); // adjust contrast

                Color baseColor = (penaltyGradient != null) ? penaltyGradient.Evaluate(t) : Color.Lerp(Color.white, Color.black, t);

                if (!n.walkable) baseColor = Color.red;

                baseColor.a = Mathf.Clamp01(shadingAlpha > 0f ? shadingAlpha : 0.6f);

                Gizmos.color = baseColor;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeDiameter * 0.98f);
            }
        }

#if UNITY_EDITOR
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
}
