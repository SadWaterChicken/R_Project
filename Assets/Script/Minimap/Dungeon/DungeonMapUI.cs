using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Cinemachine.CinemachineSplineRoll;

/// <summary>
/// Vẽ toàn bộ dungeon map lên UI Canvas.
/// Lắng nghe event từ DungeonMapManager và redraw khi state thay đổi.
///
/// Hierarchy gợi ý:
///   DungeonMapPanel (Canvas/Panel)
///   └── MapContainer (RectTransform) ← kéo vào mapContainer
///
/// Gắn script này vào DungeonMapPanel.
/// </summary>
public class DungeonMapUI : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Header("References")]
    public DungeonMapManager mapManager;

    [Tooltip("RectTransform con dùng làm canvas vẽ map")]
    public RectTransform mapContainer;

    [Header("Room Prefabs")]
    [Tooltip("Prefab phòng: Image + Text con (dùng làm label '?' / icon)")]
    public GameObject roomPrefab;

    [Tooltip("Prefab corridor: Image hình chữ nhật dài")]
    public GameObject corridorPrefab;

    [Header("Layout")]
    [Tooltip("Pixel trên UI cho mỗi ô grid")]
    public float cellSize = 24f;

    [Tooltip("Khoảng hở giữa các ô grid (corridor width)")]
    public float cellGap = 8f;

    [Header("Colors")]
    public Color colorUnexplored = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    public Color colorExplored = new Color(0.55f, 0.55f, 0.55f, 1f);
    public Color colorCurrent = new Color(0.98f, 0.83f, 0.15f, 1f);
    public Color colorBoss = new Color(0.80f, 0.10f, 0.10f, 1f);
    public Color colorShop = new Color(0.15f, 0.65f, 0.85f, 1f);
    public Color colorTreasure = new Color(0.85f, 0.65f, 0.10f, 1f);
    public Color colorCorridor = new Color(0.35f, 0.35f, 0.35f, 0.7f);

    [Header("Player Icon")]
    [Tooltip("Image icon player (mũi tên) hiển thị trên phòng hiện tại")]
    public RectTransform playerIcon;

    // -------------------------------------------------------------------------
    // Runtime
    // -------------------------------------------------------------------------
    private Dictionary<string, GameObject> _roomObjects = new();
    private Dictionary<string, GameObject> _corridorObjects = new();

    // -------------------------------------------------------------------------
    // Unity
    // -------------------------------------------------------------------------
    void Start()
    {
        if (mapManager == null)
            mapManager = DungeonMapManager.Instance;

        mapManager.onMapStateChanged.AddListener(RedrawMap);

        GenerateLayout();
        RedrawMap();
    }

    void OnDestroy()
    {
        if (mapManager != null)
            mapManager.onMapStateChanged.RemoveListener(RedrawMap);
    }

    // -------------------------------------------------------------------------
    // Layout generation (chạy một lần khi vào dungeon)
    // -------------------------------------------------------------------------
    void GenerateLayout()
    {
        ClearLayout();

        foreach (var room in mapManager.allRooms)
        {
            // --- Room object ---
            GameObject obj = Instantiate(roomPrefab, mapContainer);
            obj.name = $"Room_{room.roomId}";

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = GridToPixel(room.gridPosition);
            rt.sizeDelta = new Vector2(room.size.x * cellSize, room.size.y * cellSize);

            _roomObjects[room.roomId] = obj;

            // --- Corridors ---
            foreach (string neighborId in room.connectedRoomIds)
            {
                // Vẽ mỗi corridor một lần (tránh duplicate)
                string corridorKey = CorridorKey(room.roomId, neighborId);
                if (_corridorObjects.ContainsKey(corridorKey)) continue;

                RoomData neighbor = mapManager.GetRoom(neighborId);
                if (neighbor == null) continue;

                GameObject corr = Instantiate(corridorPrefab, mapContainer);
                corr.name = $"Corridor_{corridorKey}";
                corr.transform.SetAsFirstSibling(); // corridors dưới rooms

                RectTransform crt = corr.GetComponent<RectTransform>();
                crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0f, 0f);

                LayoutCorridor(crt, room, neighbor);

                Image corrImg = corr.GetComponent<Image>();
                if (corrImg) corrImg.color = colorCorridor;

                _corridorObjects[corridorKey] = corr;
            }
        }
    }

    void LayoutCorridor(RectTransform rt, RoomData a, RoomData b)
    {
        Vector2 posA = GridToPixel(a.gridPosition) + new Vector2(a.size.x * cellSize * 0.5f, a.size.y * cellSize * 0.5f);
        Vector2 posB = GridToPixel(b.gridPosition) + new Vector2(b.size.x * cellSize * 0.5f, b.size.y * cellSize * 0.5f);

        bool isHorizontal = Mathf.Abs(posB.x - posA.x) >= Mathf.Abs(posB.y - posA.y);

        if (isHorizontal)
        {
            float left = Mathf.Min(posA.x, posB.x) + cellSize * 0.5f;
            float width = Mathf.Abs(posB.x - posA.x) - cellSize;
            rt.anchoredPosition = new Vector2(left, posA.y - cellGap * 0.5f);
            rt.sizeDelta = new Vector2(Mathf.Max(cellGap, width), cellGap);
        }
        else
        {
            float bottom = Mathf.Min(posA.y, posB.y) + cellSize * 0.5f;
            float height = Mathf.Abs(posB.y - posA.y) - cellSize;
            rt.anchoredPosition = new Vector2(posA.x - cellGap * 0.5f, bottom);
            rt.sizeDelta = new Vector2(cellGap, Mathf.Max(cellGap, height));
        }
    }

    // -------------------------------------------------------------------------
    // Redraw (gọi mỗi khi map state thay đổi)
    // -------------------------------------------------------------------------
    public void RedrawMap()
    {
        foreach (var room in mapManager.allRooms)
        {
            if (!_roomObjects.TryGetValue(room.roomId, out GameObject obj)) continue;

            Image img = obj.GetComponent<Image>();
            Text lbl = obj.GetComponentInChildren<Text>(includeInactive: true);

            // --- Màu phòng ---
            if (room.isCurrentRoom)
            {
                img.color = colorCurrent;
            }
            else if (room.isExplored)
            {
                img.color = room.type switch
                {
                    RoomType.Boss => colorBoss,
                    RoomType.Shop => colorShop,
                    RoomType.Treasure => colorTreasure,
                    _ => colorExplored
                };
            }
            else
            {
                img.color = colorUnexplored;
            }

            // --- Label ---
            if (lbl != null)
            {
                if (room.isCurrentRoom)
                {
                    lbl.text = "";          // icon player sẽ hiển thị riêng
                    lbl.gameObject.SetActive(false);
                }
                else if (!room.isExplored)
                {
                    lbl.text = "?";
                    lbl.gameObject.SetActive(true);
                }
                else
                {
                    // Explored: hiện icon nhỏ theo type
                    lbl.text = room.type switch
                    {
                        RoomType.Boss => "☠",
                        RoomType.Shop => "$",
                        RoomType.Treasure => "★",
                        RoomType.Exit => "▶",
                        RoomType.Start => "◉",
                        _ => ""
                    };
                    lbl.gameObject.SetActive(lbl.text != "");
                }
            }
        }

        // --- Di chuyển player icon ---
        UpdatePlayerIcon();
    }

    void UpdatePlayerIcon()
    {
        if (playerIcon == null) return;

        string currentId = mapManager.CurrentRoomId;
        if (string.IsNullOrEmpty(currentId))
        {
            playerIcon.gameObject.SetActive(false);
            return;
        }

        RoomData current = mapManager.GetRoom(currentId);
        if (current == null) return;

        // Đặt icon vào tâm phòng hiện tại
        Vector2 center = GridToPixel(current.gridPosition)
                       + new Vector2(current.size.x * cellSize * 0.5f, current.size.y * cellSize * 0.5f);

        playerIcon.anchoredPosition = center;
        playerIcon.gameObject.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Rebuild hoàn toàn layout (dùng sau khi generate dungeon mới)</summary>
    public void Rebuild()
    {
        GenerateLayout();
        RedrawMap();
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------
    Vector2 GridToPixel(Vector2Int gridPos)
    {
        float step = cellSize + cellGap;
        return new Vector2(gridPos.x * step, gridPos.y * step);
    }

    static string CorridorKey(string a, string b) =>
        string.Compare(a, b, System.StringComparison.Ordinal) < 0 ? $"{a}_{b}" : $"{b}_{a}";

    void ClearLayout()
    {
        foreach (var go in _roomObjects.Values) if (go) Destroy(go);
        foreach (var go in _corridorObjects.Values) if (go) Destroy(go);
        _roomObjects.Clear();
        _corridorObjects.Clear();
    }
}