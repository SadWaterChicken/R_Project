using UnityEngine;

/// <summary>
/// Gắn vào MinimapCamera GameObject (cùng với MinimapController).
/// Xử lý việc camera follow target một cách mượt mà,
/// tách riêng để dễ swap target (spectator, cutscene...).
/// </summary>
public class MinimapFollow : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("Độ cao camera so với target (trục Y)")]
    public float height = 50f;

    [Tooltip("Tốc độ lerp vị trí. 0 = instant, cao hơn = mượt hơn")]
    [Range(0f, 20f)]
    public float followSpeed = 0f;   // 0 = snap ngay (thường dùng cho minimap)

    [Tooltip("Nếu true: camera chỉ follow trên mặt phẳng XZ, giữ nguyên Y của chính nó")]
    public bool lockVertical = true;

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------
    private Vector3 _velocity = Vector3.zero;

    // -------------------------------------------------------------------------
    // Unity
    // -------------------------------------------------------------------------
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = new Vector3(
            target.position.x,
            lockVertical ? (target.position.y + height) : target.position.y,
            target.position.z
        );

        if (followSpeed <= 0f)
        {
            // Snap ngay lập tức
            transform.position = desiredPos;
        }
        else
        {
            // SmoothDamp cho chuyển động mượt
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPos,
                ref _velocity,
                1f / followSpeed
            );
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        _velocity = Vector3.zero;

        // Snap ngay về target mới để tránh pan dài
        if (newTarget != null)
        {
            transform.position = new Vector3(
                newTarget.position.x,
                newTarget.position.y + height,
                newTarget.position.z
            );
        }
    }

    public void SetHeight(float newHeight)
    {
        height = newHeight;
    }
}
