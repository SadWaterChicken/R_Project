using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DungeonSpawnerManager : MonoBehaviour
{
    [Header("Zone Theme Setup")]
    [Tooltip("Theme (Đại tội) của khu vực này. Tất cả các cổng sinh ra trong vùng này sẽ mang theme này.")]
    public DungeonThemeSetup zoneTheme;

    [Header("Spawner Settings")]
    public GameObject entrancePrefab;
    
    [Tooltip("Thời gian (giây) giữa mỗi đợt sinh cổng")]
    public float spawnInterval = 30f;
    
    [Tooltip("Số lượng cổng tối đa tồn tại cùng lúc trong khu vực này")]
    public int maxActiveEntrances = 5;
    
    [Tooltip("Khoảng cách tối thiểu giữa cổng mới và các vật thể/cổng khác (VD: 50f)")]
    public float minSafeDistance = 50f;
    
    [Tooltip("Layer của mặt đất/địa hình. Tia raycast sẽ chỉ bắn trúng layer này để tìm mặt đất.")]
    public LayerMask terrainLayer;
    
    [Tooltip("Layer của các vật cản (nhà cửa, cây cối, đá...) để tránh sinh cổng đè lên")]
    public LayerMask obstacleLayer;

    private Collider spawnAreaCollider;
    private List<GameObject> activeEntrances = new List<GameObject>();

    private void Awake()
    {
        spawnAreaCollider = GetComponent<Collider>();
        if (spawnAreaCollider != null && !spawnAreaCollider.isTrigger)
        {
            Debug.LogWarning($"[{gameObject.name}] Collider nên được set thành IsTrigger để không cản đường người chơi!");
        }
    }

    private void Start()
    {
        if (zoneTheme == null)
        {
            Debug.LogError($"[{gameObject.name}] Chưa thiết lập Zone Theme (Đại tội) cho khu vực này!");
        }

        // Khôi phục các cổng đã lưu từ GameStateManager
        if (GameStateManager.Instance != null && GameStateManager.Instance.activeDungeons != null)
        {
            foreach (var savedData in GameStateManager.Instance.activeDungeons)
            {
                if (spawnAreaCollider != null && spawnAreaCollider.bounds.Contains(savedData.position))
                {
                    GameObject newEntrance = Instantiate(entrancePrefab, savedData.position, Quaternion.identity);
                    OverworldDungeonEntrance entranceScript = newEntrance.GetComponent<OverworldDungeonEntrance>();
                    if (entranceScript != null)
                    {
                        entranceScript.dungeonInstanceID = savedData.dungeonInstanceID;
                        entranceScript.assignedTheme = zoneTheme;
                        entranceScript.difficulty = savedData.difficulty;
                    }
                    activeEntrances.Add(newEntrance);
                }
            }
        }
        
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        TrySpawnEntrance();

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            activeEntrances.RemoveAll(item => item == null);

            if (activeEntrances.Count < maxActiveEntrances)
            {
                TrySpawnEntrance();
            }
        }
    }

    private void TrySpawnEntrance()
    {
        if (entrancePrefab == null || spawnAreaCollider == null) return;

        Vector3 spawnPosition = Vector3.zero;
        bool foundValidPosition = false;
        
        Bounds bounds = spawnAreaCollider.bounds;

        for (int i = 0; i < 30; i++)
        {
            Vector3 testPos = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.max.y + 100f,
                Random.Range(bounds.min.z, bounds.max.z)
            );
            
            // Chỉ bắt Raycast chạm vào mặt đất (TerrainLayer)
            if (Physics.Raycast(testPos, Vector3.down, out RaycastHit hit, 300f, terrainLayer))
            {
                testPos = hit.point;
                
                bool isInsideZone = true;
                if (testPos.x < bounds.min.x || testPos.x > bounds.max.x ||
                    testPos.z < bounds.min.z || testPos.z > bounds.max.z)
                {
                    isInsideZone = false;
                }
                else
                {
                    Vector3 centerCheckPos = new Vector3(testPos.x, bounds.center.y, testPos.z);
                    Vector3 closest = spawnAreaCollider.ClosestPoint(centerCheckPos);
                    if (Vector3.Distance(centerCheckPos, closest) > 0.1f)
                    {
                        isInsideZone = false;
                    }
                }

                if (isInsideZone && IsValidPosition(testPos))
                {
                    spawnPosition = testPos;
                    foundValidPosition = true;
                    break;
                }
            }
        }

        if (foundValidPosition)
        {
            GameObject newEntrance = Instantiate(entrancePrefab, spawnPosition, Quaternion.identity);
            
            OverworldDungeonEntrance entranceScript = newEntrance.GetComponent<OverworldDungeonEntrance>();
            if (entranceScript != null)
            {
                entranceScript.dungeonInstanceID = System.Guid.NewGuid().ToString();
                entranceScript.assignedTheme = zoneTheme;
                entranceScript.difficulty = (DungeonDifficultyTier)Random.Range(0, 4);

                // Add to persistent save state
                if (GameStateManager.Instance != null)
                {
                    SavedDungeonData savedData = new SavedDungeonData
                    {
                        dungeonInstanceID = entranceScript.dungeonInstanceID,
                        position = spawnPosition,
                        difficulty = entranceScript.difficulty
                    };
                    GameStateManager.Instance.activeDungeons.Add(savedData);
                    GameStateManager.Instance.SaveDungeons();
                }
            }

            activeEntrances.Add(newEntrance);
        }
    }

    private bool IsValidPosition(Vector3 pos)
    {
        foreach (var entrance in activeEntrances)
        {
            if (entrance != null && Vector3.Distance(pos, entrance.transform.position) < minSafeDistance)
                return false;
        }

        if (Physics.CheckSphere(pos, minSafeDistance, obstacleLayer))
            return false;

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.3f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
