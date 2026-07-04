using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FireBladeSlash — Skill kiếm: chém ra đường lửa thẳng với damage falloff theo khoảng cách từ tâm.
/// Để lại vệt lửa FireTrailZone dọc theo đường bay.
/// Tự động lấy SwordSkillModifiers từ SwordSkillTreeManager để điều chỉnh hành vi.
/// </summary>
public class FireBladeSlashSkill : BaseSkill
{
    // ─── ĐƯỢC SET TỰ ĐỘNG BỞI SwordSkillTreeManager ─────────────────────────────
    [HideInInspector] public SwordSkillModifiers modifiers;

    // ─── BASE PARAMS ─────────────────────────────────────────────────────────
    [Header("Base Movement")]
    public float baseSpeed = 16f;
    public float baseMaxRange = 20f;
    public float spawnHeightOffset = 1.1f;
    public float spawnForwardOffset = 0.9f;
    public bool alignToPlayerFacing = true;

    [Header("Base Hitbox")]
    public Vector3 baseHitboxSize = new Vector3(2.2f, 2f, 6f);  // x=width, y=height, z=depth/length
    public Vector3 hitboxOffset = Vector3.zero;
    public LayerMask enemyLayers = ~0;

    [Header("Damage Falloff")]
    public float coreDamageRadius = 2.0f;       // 100%
    public float midDamageRadius = 4f;           // 65%
    [Range(0f, 1f)] public float midDamageRatio = 0.65f;
    [Range(0f, 1f)] public float edgeDamageRatio = 0.35f;

    [Header("Fire Trail")]
    public GameObject fireTrailZonePrefab;
    public float baseFireTrailDuration = 8f;     // Tăng lên rất lâu để thấy rõ
    public float trailSpawnInterval = 0.15f;     // Dày hơn 1 chút (trước là 0.18f)

    [Header("DoT")]
    public float baseDotDPS = 10f;
    public float baseDotDuration = 4f;

    [Header("VFX")]
    public GameObject fireSlashVFX;              // Slash Fire VFX.prefab
    public GameObject impactVFX;                 // Impact.prefab
    public float vfxScale = 4f;                  // Chỉnh to ra 1 chút
    public Vector3 vfxRotationOffset = new Vector3(0, 0, 0); // Thêm dòng này để dễ xoay VFX

    [Header("Light")]
    public bool addTravelLight = true;
    public Color travelLightColor = new Color(1f, 0.4f, 0.1f, 1f);
    public float travelLightIntensity = 3f;
    public float travelLightRange = 5f;

    // ─── RUNTIME ─────────────────────────────────────────────────────────────
    private bool executed;
    private Vector3 startPosition;
    private float currentSpeed;
    private float currentMaxRange;
    private Vector3 currentHitboxSize;
    private float currentFireTrailDuration;
    private float currentDotDPS;
    private float currentDotDuration;

    private float trailTimer;
    private readonly HashSet<CharacterStats> hitEnemies = new HashSet<CharacterStats>();

    // ─── EXECUTE ─────────────────────────────────────────────────────────────
    public override void ExecuteSkill()
    {
        if (executed) return;
        executed = true;

        // ── 0. Lấy Modifier từ hệ thống ──
        modifiers = SwordSkillTreeManager.Instance != null
            ? SwordSkillTreeManager.Instance.GetCurrentModifiers()
            : SwordSkillModifiers.Default();

        // ── 1. Tính params theo modifiers ──
        currentSpeed    = baseSpeed;
        currentMaxRange = baseMaxRange;
        currentHitboxSize = baseHitboxSize;  // FireBladeSlash là AoE mặc định từ đầu

        // DamageUp: +40%
        damageMultiplier = 1.5f;
        if (modifiers.fireDamageUp) damageMultiplier *= 1.4f;

        // CritUp: ghi nhớ để TryHitEnemy tự tung cắt
        // (critChance xử lý trong TryHitEnemy)

        // FireTrail
        currentFireTrailDuration = baseFireTrailDuration;

        // DoT
        currentDotDPS      = baseDotDPS;
        currentDotDuration = baseDotDuration;
        if (modifiers.fireBurnDuration)
            currentDotDuration = 5f;   // 3s → 5s

        // TwinInferno: mở rộng hitbox
        if (modifiers.twinInferno)
            currentHitboxSize = new Vector3(baseHitboxSize.x * 1.5f, baseHitboxSize.y, baseHitboxSize.z);

        // ── 2. Alignment với hướng player ──
        if (alignToPlayerFacing)
        {
            Vector3 dir = GetPlayerFacingDirection();
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
                if (PlayerStat.Instance != null)
                    transform.position = PlayerStat.Instance.transform.position
                                         + Vector3.up * spawnHeightOffset
                                         + dir * spawnForwardOffset;
            }
        }
        startPosition = transform.position;

        // ── 3. Spawn VFX ──
        SpawnFireVFX();
        if (addTravelLight) AddTravelLight();

        // ── 4. Twin Inferno: spawn lần 2 có delay nhỏ ──
        if (modifiers.twinInferno)
            StartCoroutine(SpawnTwinSlash());

        // ── 5. Auto-destroy ──
        float lifetime = currentMaxRange / Mathf.Max(currentSpeed, 0.1f) + 0.5f;
        Destroy(gameObject, lifetime);
    }

    private System.Collections.IEnumerator SpawnTwinSlash()
    {
        yield return new WaitForSeconds(0.35f); // delay 0.35s sau lần 1

        Vector3 dir = GetPlayerFacingDirection();
        Vector3 spawnPos = PlayerStat.Instance != null
            ? PlayerStat.Instance.transform.position + Vector3.up * spawnHeightOffset + dir * spawnForwardOffset
            : transform.position;

        GameObject twin = Instantiate(gameObject, spawnPos, Quaternion.LookRotation(dir));
        FireBladeSlashSkill twinSkill = twin.GetComponent<FireBladeSlashSkill>();
        if (twinSkill != null)
        {
            // Disable TwinInferno ở instance thứ 2 để tránh vòng lặp vô tận
            twinSkill.modifiers = modifiers;
            twinSkill.modifiers.twinInferno = false;
            twinSkill.Initialize(caster, damageMultiplier, weaponPhysicalDamage);
            twinSkill.ExecuteSkill();
        }
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────
    private void Update()
    {
        if (!executed) return;

        // Di chuyển
        transform.position += transform.forward * currentSpeed * Time.deltaTime;

        // Kiểm tra đã đi đủ tầm chưa
        if (Vector3.Distance(startPosition, transform.position) >= currentMaxRange)
        {
            SpawnImpactVFX(transform.position);
            Destroy(gameObject);
            return;
        }

        // Spawn vệt lửa
        trailTimer += Time.deltaTime;
        if (trailTimer >= trailSpawnInterval)
        {
            trailTimer = 0f;
            SpawnFireTrail();
        }

        // Scan damage
        ScanForEnemies();
    }

    // ─── DAMAGE ───────────────────────────────────────────────────────────────
    private void ScanForEnemies()
    {
        Vector3 center = transform.TransformPoint(hitboxOffset);
        Collider[] hits = Physics.OverlapBox(
            center,
            currentHitboxSize * 0.5f,
            transform.rotation,
            enemyLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            TryHitEnemy(hit, center);
        }
    }

    private void TryHitEnemy(Collider other, Vector3 slashCenter)
    {
        CharacterStats stats = other.GetComponentInParent<CharacterStats>();
        if (stats == null || stats is PlayerStat) return;
        if (hitEnemies.Contains(stats)) return;

        hitEnemies.Add(stats);

        // Damage falloff theo khoảng cách từ tâm slash
        float dist  = Vector3.Distance(other.ClosestPoint(slashCenter), slashCenter);
        float ratio = GetDamageFalloffRatio(dist);
        float finalDamage = CalculatePhysicalDamage() * ratio;

        // CritUp: +20% chí mạng — tính ngẫu nhiên
        if (modifiers.fireCritUp)
        {
            float critChance = (PlayerStat.Instance != null ? PlayerStat.Instance.critChance : 0f) + 0.2f;
            if (UnityEngine.Random.value < critChance)
            {
                finalDamage *= 1.5f;
                Debug.Log("[FireBladeSlash] Critical Hit!");
            }
        }

        float hpBefore = stats.currentHealth;
        stats.TakePhysicalDamage(finalDamage);
        
        if (hpBefore > 0 && stats.currentHealth <= 0)
        {
            RewardMasteryOnSkillKill(other.gameObject, stats);
        }
        
        ApplyBurnToEnemy(stats);
        SpawnImpactVFX(other.ClosestPoint(slashCenter));
    }

    private float GetDamageFalloffRatio(float distance)
    {
        if (distance <= coreDamageRadius) return 1.0f;
        if (distance <= midDamageRadius)  return midDamageRatio;
        return edgeDamageRatio;
    }

    private void ApplyBurnToEnemy(CharacterStats stats)
    {
        // Renew thay vì stack
        FireBurnDoT existing = stats.GetComponent<FireBurnDoT>();
        if (existing != null) Destroy(existing);

        FireBurnDoT dot = stats.gameObject.AddComponent<FireBurnDoT>();
        dot.Initialize(stats, currentDotDPS, currentDotDuration);
    }

    // ─── VFX / TRAIL ─────────────────────────────────────────────────────────
    private void SpawnFireVFX()
    {
        if (fireSlashVFX == null) return;

        GameObject vfx = Instantiate(fireSlashVFX, transform);
        vfx.transform.localPosition = Vector3.zero;
        vfx.transform.localRotation = Quaternion.Euler(vfxRotationOffset);

        float scale = vfxScale;
        vfx.transform.localScale = Vector3.one * scale;

        // Bật tất cả particle
        foreach (var p in vfx.GetComponentsInChildren<ParticleSystem>())
            p.Play(true);

        // Gỡ script Projectile của VFX asset để không bị conflict
        foreach (var script in vfx.GetComponentsInChildren<MonoBehaviour>())
        {
            if (script.GetType().Name.Contains("Projectile") || script.GetType().Name.Contains("Move"))
            {
                script.enabled = false;
                Destroy(script);
            }
        }
    }

    private void SpawnFireTrail()
    {
        if (fireTrailZonePrefab == null) return;

        // Spawn tại vị trí hiện tại (hơi thấp hơn, áp sát đất)
        Vector3 spawnPos = transform.position - Vector3.up * 0.8f;
        GameObject trail = Instantiate(fireTrailZonePrefab, spawnPos, Quaternion.identity);

        FireTrailZone zone = trail.GetComponent<FireTrailZone>();
        if (zone != null)
        {
            zone.zoneDuration = currentFireTrailDuration;
            zone.damagePerSecond = currentDotDPS;
            zone.dotDuration = currentDotDuration;
        }
    }

    private void SpawnImpactVFX(Vector3 position)
    {
        if (impactVFX == null) return;
        GameObject impact = Instantiate(impactVFX, position, Quaternion.identity);
        Destroy(impact, 2f);
    }

    private void AddTravelLight()
    {
        Light light = gameObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = travelLightColor;
        light.intensity = travelLightIntensity;
        light.range = travelLightRange;
    }

    // ─── GIZMO ───────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Vector3 size = Application.isPlaying ? currentHitboxSize : baseHitboxSize;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.5f);
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(hitboxOffset), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
        Gizmos.matrix = old;

        // Vẽ vòng tròn falloff
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, coreDamageRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, midDamageRadius);
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────
    private Vector3 GetPlayerFacingDirection()
    {
        Transform activeCam = Camera.main != null ? Camera.main.transform : null;
        if (activeCam != null)
        {
            Vector3 dir = activeCam.forward;
            dir.y = 0f;
            dir.Normalize();
            if (dir.sqrMagnitude > 0.001f) return dir;
        }

        PlayerStat player = PlayerStat.Instance;
        if (player != null)
        {
            Vector3 fallback = player.transform.forward;
            fallback.y = 0f;
            if (fallback.sqrMagnitude > 0.001f) return fallback.normalized;
        }
        return Vector3.forward;
    }
}
