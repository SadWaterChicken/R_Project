using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vùng lửa được để lại dọc theo đường bay của FireBladeSlash.
/// Kẻ địch đứng trong vùng sẽ bị DoT lửa.
/// </summary>
public class FireTrailZone : MonoBehaviour
{
    [Header("DoT Settings")]
    public float damagePerSecond = 8f;
    public float dotDuration = 3f;         // DoT kéo dài bao lâu trên địch
    public float zoneDuration = 3f;        // Bao lâu thì vùng lửa biến mất
    public LayerMask enemyLayers = ~0;

    [Header("Visuals")]
    public float zoneRadius = 1.0f;        // Bán kính vùng lửa (cho Gizmo)
    public bool addLight = true;
    public Color lightColor = new Color(1f, 0.4f, 0f, 1f);
    public float lightIntensity = 2f;
    public float lightRange = 4f;

    private float elapsed;
    private readonly HashSet<CharacterStats> burnedThisFrame = new HashSet<CharacterStats>();

    private void Start()
    {
        if (addLight && GetComponent<Light>() == null)
        {
            Light l = gameObject.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = lightColor;
            l.intensity = lightIntensity;
            l.range = lightRange;
            
            // Đặt light thấp xuống để hắt từ dưới đất lên
            l.transform.localPosition = new Vector3(0, 0.2f, 0);
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        burnedThisFrame.Clear();

        if (elapsed >= zoneDuration)
        {
            Destroy(gameObject);
            return;
        }

        // Phát hiện kẻ địch đứng trong vùng
        Collider[] hits = Physics.OverlapSphere(transform.position, zoneRadius, enemyLayers, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            CharacterStats stats = hit.GetComponentInParent<CharacterStats>();
            if (stats == null || stats is PlayerStat) continue;
            if (burnedThisFrame.Contains(stats)) continue;

            burnedThisFrame.Add(stats);
            ApplyBurnToEnemy(stats);
        }
    }

    private void ApplyBurnToEnemy(CharacterStats stats)
    {
        // Nếu đã có DoT thì renew thay vì stack thêm
        FireBurnDoT existing = stats.GetComponent<FireBurnDoT>();
        if (existing != null)
        {
            Destroy(existing);
        }

        FireBurnDoT dot = stats.gameObject.AddComponent<FireBurnDoT>();
        dot.Initialize(stats, damagePerSecond, dotDuration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawSphere(transform.position, zoneRadius);
    }
}
