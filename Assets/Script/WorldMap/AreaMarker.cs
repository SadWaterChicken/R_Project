using UnityEngine;

public class AreaMarker : MonoBehaviour
{
    public enum ZoneType { SafeZone, EnemySpawnZone }

    [Header("Cấu hình Zone")]
    public ZoneType zoneType = ZoneType.SafeZone;
    public float radius = 5f; // Bán kính vùng

    // Vẽ vòng tròn trực quan trong Scene View để team thiết kế dễ nhìn
    private void OnDrawGizmos()
    {
        if (zoneType == ZoneType.SafeZone)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Màu xanh lá trong suốt
        }
        else
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Màu đỏ trong suốt
        }

        // Vẽ hình cầu biểu thị vùng hoạt động
        Gizmos.DrawSphere(transform.position, radius);

        // Vẽ thêm viền đậm
        Gizmos.color = zoneType == ZoneType.SafeZone ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}