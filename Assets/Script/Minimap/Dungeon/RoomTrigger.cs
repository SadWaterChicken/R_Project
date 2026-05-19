using UnityEngine;

/// <summary>
/// Gắn vào GameObject đại diện cho mỗi phòng trong dungeon (cần có Collider với Is Trigger = true).
/// Khi player bước vào, thông báo cho DungeonMapManager.
///
/// Setup:
///   1. Thêm BoxCollider (hoặc collider khác) vào room GameObject → bật Is Trigger
///   2. Gắn RoomTrigger vào cùng GameObject
///   3. Điền roomId trong Inspector (phải khớp với RoomData.roomId)
///   4. Player phải có tag "Player" và Rigidbody (hoặc Character Controller)
/// </summary>
[RequireComponent(typeof(Collider))]
public class RoomTrigger : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Tooltip("ID phòng này — phải khớp với RoomData.roomId trong DungeonMapManager")]
    public string roomId;

    [Tooltip("Tag của player GameObject")]
    public string playerTag = "Player";

    [Header("Optional")]
    [Tooltip("Nếu bật: log ra console khi player vào phòng (debug)")]
    public bool debugLog = false;

    // -------------------------------------------------------------------------
    // Unity
    // -------------------------------------------------------------------------
    void Awake()
    {
        // Đảm bảo collider là trigger
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[RoomTrigger] {gameObject.name}: Collider was not a trigger — auto-fixed.");
        }

        if (string.IsNullOrEmpty(roomId))
            Debug.LogError($"[RoomTrigger] {gameObject.name}: roomId chưa được điền!", this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (debugLog)
            Debug.Log($"[RoomTrigger] Player vào phòng: {roomId}");

        if (DungeonMapManager.Instance == null)
        {
            Debug.LogError("[RoomTrigger] DungeonMapManager.Instance is null!");
            return;
        }

        DungeonMapManager.Instance.EnterRoom(roomId);
    }

    // -------------------------------------------------------------------------
    // Editor helper
    // -------------------------------------------------------------------------
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Vẽ label roomId trong Scene view để dễ debug
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"[{roomId}]");
    }
#endif
}