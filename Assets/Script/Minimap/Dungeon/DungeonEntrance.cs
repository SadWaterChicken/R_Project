using UnityEngine;

/// <summary>
/// Gắn vào GameObject cửa vào dungeon (cần Collider với Is Trigger = true).
/// Chịu trách nhiệm:
///   1. Báo MapModeController chuyển sang dungeon map
///   2. Báo DungeonMapManager reset + bắt đầu ở phòng đầu tiên
///   3. (Optional) Notify khi thoát dungeon
///
/// Tách khỏi MapModeController để mỗi dungeon có entrance riêng,
/// không cần sửa UI manager.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DungeonEntrance : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Header("References")]
    [Tooltip("MapModeController trên HUD Canvas")]
    public MapModeController mapModeController;

    [Header("Dungeon Config")]
    [Tooltip("roomId của phòng đầu tiên player xuất hiện khi vào dungeon")]
    public string startRoomId = "room_start";

    [Tooltip("Nếu true: reset toàn bộ explored state mỗi lần vào dungeon")]
    public bool resetOnEnter = true;

    [Header("Exit")]
    [Tooltip("Nếu object này cũng là exit (2 chiều), bật cờ này")]
    public bool isBidirectional = false;

    [Header("Detection")]
    public string playerTag = "Player";
    public bool debugLog = false;

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------
    private bool _playerInside = false;

    // -------------------------------------------------------------------------
    // Unity
    // -------------------------------------------------------------------------
    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[DungeonEntrance] {gameObject.name}: Collider auto-set to trigger.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || _playerInside) return;
        _playerInside = true;

        if (debugLog) Debug.Log($"[DungeonEntrance] Player entering dungeon via {gameObject.name}");

        HandleEnterDungeon();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag) || !_playerInside) return;
        _playerInside = false;

        if (isBidirectional)
        {
            if (debugLog) Debug.Log($"[DungeonEntrance] Player exiting dungeon via {gameObject.name}");
            HandleExitDungeon();
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------
    void HandleEnterDungeon()
    {
        // 1. Reset map nếu cần
        if (resetOnEnter && DungeonMapManager.Instance != null)
            DungeonMapManager.Instance.ResetAll();

        // 2. Chuyển UI sang dungeon mode
        if (mapModeController != null)
            mapModeController.EnterDungeon();
        else
            Debug.LogWarning("[DungeonEntrance] mapModeController chưa được gán!");

        // 3. Đánh dấu phòng bắt đầu
        if (DungeonMapManager.Instance != null)
            DungeonMapManager.Instance.EnterRoom(startRoomId);
        else
            Debug.LogWarning("[DungeonEntrance] DungeonMapManager.Instance is null!");
    }

    void HandleExitDungeon()
    {
        if (mapModeController != null)
            mapModeController.ExitDungeon();
    }

    // -------------------------------------------------------------------------
    // Public API (gọi từ code khác nếu cần trigger manually)
    // -------------------------------------------------------------------------
    public void TriggerEnter() => HandleEnterDungeon();
    public void TriggerExit() => HandleExitDungeon();

    // -------------------------------------------------------------------------
    // Editor
    // -------------------------------------------------------------------------
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f,
            $"Dungeon Entrance\nStart: {startRoomId}");
    }
#endif
}
