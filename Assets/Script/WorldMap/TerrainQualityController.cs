// TerrainQualityController.cs
using UnityEngine;

public class TerrainQualityController : MonoBehaviour
{
    [Header("Terrain Reference")]
    public Terrain terrain;

    [Header("FPS Thresholds")]
    public float targetFPS = 60f;
    public float lowFPS = 30f;

    [Header("Detail Settings")]
    public float highDetailDistance = 80f;
    public float lowDetailDistance = 40f;
    public float highDetailDensity = 1f;
    public float lowDetailDensity = 0.4f;

    [Header("Tree Settings")]
    public float highTreeDistance = 250f;
    public float lowTreeDistance = 100f;

    private float fpsCheckInterval = 3f;
    private float timer;
    private bool isLowQuality = false;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fpsCheckInterval)
        {
            timer = 0f;
            float currentFPS = 1f / Time.smoothDeltaTime;
            AdjustQuality(currentFPS);
        }
    }

    void AdjustQuality(float fps)
    {
        if (fps < lowFPS && !isLowQuality)
        {
            // Giảm chất lượng
            terrain.detailObjectDistance = lowDetailDistance;
            terrain.detailObjectDensity = lowDetailDensity;
            terrain.treeDistance = lowTreeDistance;
            isLowQuality = true;
            Debug.Log($"[Terrain] Giảm chất lượng - FPS: {fps:F1}");
        }
        else if (fps >= targetFPS && isLowQuality)
        {
            // Phục hồi chất lượng cao
            terrain.detailObjectDistance = highDetailDistance;
            terrain.detailObjectDensity = highDetailDensity;
            terrain.treeDistance = highTreeDistance;
            isLowQuality = false;
            Debug.Log($"[Terrain] Phục hồi chất lượng - FPS: {fps:F1}");
        }
    }
}