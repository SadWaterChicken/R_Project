using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton quản lý toàn bộ state của dungeon map.
/// Biết tất cả các phòng, phòng nào đã khám phá, player đang ở đâu.
/// Không làm UI — chỉ giữ data và phát event.
///
/// Gắn vào: một GameObject tên "DungeonMapManager" trong scene Dungeon.
/// </summary>
public class DungeonMapManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static DungeonMapManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Header("Room List")]
    [Tooltip("Danh sách tất cả phòng trong dungeon. Điền trong Inspector hoặc generate runtime.")]
    public List<RoomData> allRooms = new List<RoomData>();

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------
    [Header("Events")]
    [Tooltip("Phát khi player vào phòng mới (truyền roomId)")]
    public UnityEvent<string> onRoomEntered;

    [Tooltip("Phát khi một phòng được explore lần đầu (truyền roomId)")]
    public UnityEvent<string> onRoomExplored;

    [Tooltip("Phát bất cứ khi nào map state thay đổi — DungeonMapUI lắng nghe cái này để redraw")]
    public UnityEvent onMapStateChanged;

    // -------------------------------------------------------------------------
    // Runtime state
    // -------------------------------------------------------------------------
    private string _currentRoomId;
    private Dictionary<string, RoomData> _roomLookup;

    // -------------------------------------------------------------------------
    // Unity
    // -------------------------------------------------------------------------
    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildLookup();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gọi khi player bước vào phòng.
    /// RoomTrigger và DungeonEntrance gọi hàm này.
    /// </summary>
    public void EnterRoom(string roomId)
    {
        // Bỏ đánh dấu phòng cũ
        if (!string.IsNullOrEmpty(_currentRoomId))
        {
            RoomData prev = GetRoom(_currentRoomId);
            if (prev != null) prev.isCurrentRoom = false;
        }

        RoomData room = GetRoom(roomId);
        if (room == null)
        {
            Debug.LogWarning($"[DungeonMapManager] Room not found: {roomId}");
            return;
        }

        bool firstVisit = !room.isExplored;

        room.isExplored = true;
        room.isCurrentRoom = true;
        _currentRoomId = roomId;

        // Events
        onRoomEntered?.Invoke(roomId);
        if (firstVisit) onRoomExplored?.Invoke(roomId);
        onMapStateChanged?.Invoke();
    }

    /// <summary>Lấy RoomData theo ID. Trả null nếu không tìm thấy.</summary>
    public RoomData GetRoom(string roomId)
    {
        if (_roomLookup == null) BuildLookup();
        return _roomLookup.TryGetValue(roomId, out var r) ? r : null;
    }

    public string CurrentRoomId => _currentRoomId;

    /// <summary>Reset toàn bộ explored state (dùng khi bắt đầu dungeon mới)</summary>
    public void ResetAll()
    {
        foreach (var room in allRooms)
        {
            room.isExplored = false;
            room.isCurrentRoom = false;
        }
        _currentRoomId = null;
        onMapStateChanged?.Invoke();
    }

    /// <summary>
    /// Thêm phòng runtime (dùng khi generate dungeon procedural).
    /// Gọi RebuildLookup() sau khi add xong.
    /// </summary>
    public void AddRoom(RoomData room)
    {
        allRooms.Add(room);
    }

    public void RebuildLookup() => BuildLookup();

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------
    void BuildLookup()
    {
        _roomLookup = new Dictionary<string, RoomData>(allRooms.Count);
        foreach (var room in allRooms)
        {
            if (string.IsNullOrEmpty(room.roomId))
            {
                Debug.LogWarning("[DungeonMapManager] Room with empty ID skipped.");
                continue;
            }
            if (!_roomLookup.TryAdd(room.roomId, room))
                Debug.LogWarning($"[DungeonMapManager] Duplicate roomId: {room.roomId}");
        }
    }
}