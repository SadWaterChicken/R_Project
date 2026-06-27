using System.Collections.Generic;
using UnityEngine;

public class WindSlashSkill : BaseSkill
{
    [Header("Movement")]
    public float speed = 13f;
    public float lifetime = 1.2f;
    public float maxTravelDistance = 9f;
    public bool pierceEnemies = true;
    public bool alignToPlayerFacing = true;
    public float spawnForwardOffset = 0.8f;
    public float spawnHeightOffset = 1f;
    public float cooldown = 0f;
    public float flyingVFXScale = 3f;
    public int flyingVFXLayers = 3;
    public float layeredVFXOffset = 0.12f;
    public bool addTravelLight = true;
    public Color travelLightColor = new Color(0.2f, 0.8f, 1f, 1f);
    public float travelLightIntensity = 3.5f;
    public float travelLightRange = 6f;

    [Header("Hitbox")]
    public Vector3 hitboxSize = new Vector3(5f, 2f, 1.8f);
    public Vector3 hitboxOffset = Vector3.zero;
    public LayerMask enemyLayers = ~0;

    [Header("Fallback Damage")]
    public float fallbackDamageMultiplier = 2f;
    public float fallbackWeaponPhysicalDamage;

    [Header("VFX")]
    public GameObject flyingVFX;
    public GameObject hitVFX;
    public bool spawnHitVFX = false;

    private readonly HashSet<EnemyStat> hitEnemies = new HashSet<EnemyStat>();
    private bool executed;
    private Vector3 startPosition;
    private static float lastCastTime = -999f;

    private void Start()
    {
        if (!executed)
        {
            Initialize(PlayerStat.Instance, fallbackDamageMultiplier, fallbackWeaponPhysicalDamage);
            ExecuteSkill();
        }
    }

    public override void ExecuteSkill()
    {
        if (executed) return;

        if (Time.time < lastCastTime + cooldown)
        {
            Destroy(gameObject);
            return;
        }

        lastCastTime = Time.time;
        executed = true;

        if (alignToPlayerFacing)
        {
            Vector3 direction = GetPlayerFacingDirection();
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
                if (PlayerStat.Instance != null)
                {
                    transform.position = PlayerStat.Instance.transform.position + Vector3.up * spawnHeightOffset + direction * spawnForwardOffset;
                }
            }
        }

        startPosition = transform.position;

        if (flyingVFX != null)
        {
            SpawnLayeredFlyingVFX();
        }

        if (addTravelLight)
        {
            Light travelLight = gameObject.AddComponent<Light>();
            travelLight.type = LightType.Point;
            travelLight.color = travelLightColor;
            travelLight.intensity = travelLightIntensity;
            travelLight.range = travelLightRange;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!executed) return;

        transform.position += transform.forward * speed * Time.deltaTime;
        if (Vector3.Distance(startPosition, transform.position) >= maxTravelDistance)
        {
            Destroy(gameObject);
            return;
        }

        ScanForEnemies();
    }

    private void ScanForEnemies()
    {
        Vector3 center = transform.TransformPoint(hitboxOffset);
        Collider[] hits = Physics.OverlapBox(center, hitboxSize * 0.5f, transform.rotation, enemyLayers, QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            TryHitEnemy(hit);
        }
    }

    private void TryHitEnemy(Collider other)
    {
        EnemyStat enemy = other.GetComponentInParent<EnemyStat>();
        if (enemy == null || hitEnemies.Contains(enemy)) return;

        hitEnemies.Add(enemy);
        enemy.TakePhysicalDamage(CalculatePhysicalDamage());

        if (spawnHitVFX && hitVFX != null)
        {
            Instantiate(hitVFX, other.ClosestPoint(transform.position), Quaternion.identity);
        }

        if (!pierceEnemies)
        {
            Destroy(gameObject);
        }
    }

    private void SpawnLayeredFlyingVFX()
    {
        int layerCount = Mathf.Max(1, flyingVFXLayers);
        for (int i = 0; i < layerCount; i++)
        {
            GameObject vfx = Instantiate(flyingVFX, transform);
            float layerOffset = (i - (layerCount - 1) * 0.5f) * layeredVFXOffset;
            vfx.transform.localPosition = new Vector3(layerOffset, 0f, 0f);
            vfx.transform.localRotation = Quaternion.Euler(0f, i * 8f, i % 2 == 0 ? 0f : 180f);
            vfx.transform.localScale = Vector3.one * flyingVFXScale * (1f + i * 0.08f);
            
            // Fix VFX not playing because it has playOnAwake = false
            ParticleSystem[] particles = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var p in particles)
            {
                p.Play(true);
            }

            // Remove interfering scripts from third-party VFX assets
            MonoBehaviour[] scripts = vfx.GetComponentsInChildren<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script.GetType().Name.Contains("Projectile"))
                {
                    Destroy(script);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(hitboxOffset), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
        Gizmos.matrix = oldMatrix;
    }

    private Vector3 GetPlayerFacingDirection()
    {
        PlayerStat player = PlayerStat.Instance;
        if (player == null) return transform.forward;

        Transform activeCamera = Camera.main != null ? Camera.main.transform : null;
        if (activeCamera != null)
        {
            Vector3 direction = activeCamera.forward;
            direction.y = 0f;
            direction.Normalize();

            return direction.sqrMagnitude > 0.001f ? direction : player.transform.forward;
        }

        Vector3 fallback = player.transform.forward;
        fallback.y = 0f;
        return fallback.sqrMagnitude > 0.001f ? fallback.normalized : Vector3.forward;
    }
}
