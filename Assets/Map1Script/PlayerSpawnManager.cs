using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    // Biến static lưu tạm tên spawn point (truy cập xuyên scene)
    public static string nextSpawnPointName;

    private void Start()
    {
        // Nếu không có spawn point nào được set -> giữ nguyên vị trí hiện tại
        if (string.IsNullOrEmpty(nextSpawnPointName)) return;

        // Tìm object spawn point trong scene
        GameObject spawnPoint = GameObject.Find(nextSpawnPointName);
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
        }
        else
        {
            Debug.LogWarning("Không tìm thấy spawn point: " + nextSpawnPointName);
        }

        // Reset để không ảnh hưởng các lần load sau
        nextSpawnPointName = null;
    }
}
