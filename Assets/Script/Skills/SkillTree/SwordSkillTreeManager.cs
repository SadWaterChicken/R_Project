using UnityEngine;

/// <summary>
/// Singleton quản lý Sword Skill Tree — 2 nhánh: WindSlash và FireBladeSlash.
/// Được tạo tự động bởi PlayerSkillCastBootstrap.
/// </summary>
public class SwordSkillTreeManager : MonoBehaviour
{
    public static SwordSkillTreeManager Instance { get; private set; }

    private SwordSkillTreeData data;

    // ─── LIFECYCLE ────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        data = SwordSkillTreeData.Load();
    }

    private void OnApplicationQuit() => data?.Save();

    // ─── PUBLIC API ───────────────────────────────────────────────────────────

    /// <summary>Thêm SP — gọi từ SwordMasteryTracker khi đủ EXP.</summary>
    public void AddSkillPoint(int amount = 1)
    {
        int maxTotal = SwordMasteryTracker.TOTAL_MAX_SP;
        int canAdd   = Mathf.Min(amount, maxTotal - data.totalSpGranted);
        if (canAdd <= 0) return;

        data.totalSpGranted       += canAdd;
        data.availableSkillPoints += canAdd;
        data.Save();
        Debug.Log($"[SwordSkillTreeManager] +{canAdd} SP | Có sẵn: {data.availableSkillPoints} | Tổng: {data.totalSpGranted}/{maxTotal}");
    }

    public int  GetAvailablePoints()         => data.availableSkillPoints;
    public SwordSkillTreeData GetData()      => data;
    public bool IsWindSlashUnlocked()        => data.windSlashUnlocked;
    public bool IsFireBladeSlashUnlocked()   => data.fireBladeSlashUnlocked;

    /// <summary>Mở 1 node. Trả về true nếu thành công.</summary>
    public bool UnlockNode(string nodeID)
    {
        int cost = SwordSkillTreeData.GetNodeCost(nodeID);
        if (data.availableSkillPoints < cost)
        {
            Debug.LogWarning($"[SwordSkillTreeManager] Không đủ SP: cần {cost}, có {data.availableSkillPoints}.");
            return false;
        }

        if (!ApplyUnlock(nodeID)) return false;

        data.availableSkillPoints -= cost;
        data.Save();
        Debug.Log($"[SwordSkillTreeManager] Mở '{nodeID}' | Còn: {data.availableSkillPoints} SP");
        return true;
    }

    /// <summary>Lấy modifier hiện tại để truyền vào skill khi cast.</summary>
    public SwordSkillModifiers GetCurrentModifiers() => new SwordSkillModifiers
    {
        // WindSlash
        windManaSave     = data.windManaSave,
        windRangeUp      = data.windRangeUp,
        windDamageUp     = data.windDamageUp,
        windCooldownDown = data.windCooldownDown,
        tempestBlade     = data.tempestBladeUnlocked,
        // FireBladeSlash
        fireManaSave     = data.fireManaSave,
        fireCritUp       = data.fireCritUp,
        fireDamageUp     = data.fireDamageUp,
        fireBurnDuration = data.fireBurnDuration,
        twinInferno      = data.twinInfernoUnlocked,
    };

    // ─── APPLY WINDSLASH MODS AT RUNTIME ─────────────────────────────────────
    /// <summary>
    /// Áp modifier WindSlash lên instance vừa spawn.
    /// Gọi từ PlayerSkillCastController ngay trước ExecuteSkill().
    /// </summary>
    public void ApplyWindSlashMods(WindSlashSkill slash, SwordSkillModifiers mods)
    {
        if (slash == null) return;

        // RangeUp: 10m → 15m
        if (mods.windRangeUp)
            slash.maxTravelDistance = 15f;

        // DamageUp: +40%
        if (mods.windDamageUp)
            slash.fallbackDamageMultiplier *= 1.4f;

        // TempestBlade: AoE (piercing), +60% dame, +6m tầm thêm, hitbox rộng hơn
        if (mods.tempestBlade)
        {
            slash.pierceEnemies       = true;
            slash.maxTravelDistance   = (mods.windRangeUp ? 15f : 10f) + 6f;
            slash.fallbackDamageMultiplier *= 1.6f;
            slash.hitboxSize = new Vector3(6f, 2f, 2f); // AoE rộng hơn
        }
    }

    // ─── APPLY FIRE MODS AT RUNTIME ──────────────────────────────────────────
    /// <summary>Áp modifier FireBladeSlash lên instance vừa spawn.
    /// Gọi từ PlayerSkillCastController trước khi ExecuteSkill().
    /// </summary>
    public void ApplyFireBladeMods(FireBladeSlashSkill fire, SwordSkillModifiers mods)
    {
        if (fire == null) return;
        // DamageUp được xử lý bên trong ExecuteSkill() của FireBladeSlashSkill (qua modifiers.fireDamageUp)
        // BurnDuration: 3s → 5s
        if (mods.fireBurnDuration)
            fire.baseDotDuration = 5f;
        // TwinInferno: mở rộng hitbox
        if (mods.twinInferno)
            fire.baseHitboxSize = new Vector3(fire.baseHitboxSize.x * 1.5f,
                                              fire.baseHitboxSize.y,
                                              fire.baseHitboxSize.z);
    }

    // ─── UNLOCK LOGIC ────────────────────────────────────────────────────────
    private bool ApplyUnlock(string nodeID)
    {
        switch (nodeID)
        {
            // ── Nhánh WindSlash (theo chuỗi) ──
            case "windSlash":
                if (data.windSlashUnlocked) return false;
                data.windSlashUnlocked = true; return true;

            case "windManaSave":
                if (data.windManaSave || !data.windSlashUnlocked) return false;
                data.windManaSave = true; return true;

            case "windRangeUp":
                if (data.windRangeUp || !data.windManaSave) return false;
                data.windRangeUp = true; return true;

            case "windDamageUp":
                if (data.windDamageUp || !data.windRangeUp) return false;
                data.windDamageUp = true; return true;

            case "windCooldownDown":
                if (data.windCooldownDown || !data.windDamageUp) return false;
                data.windCooldownDown = true; return true;

            case "tempestBlade":
                if (data.tempestBladeUnlocked || !data.windCooldownDown) return false;
                data.tempestBladeUnlocked = true; return true;

            // ── Nhánh FireBladeSlash (theo chuỗi) ──
            case "fireBladeSlash":
                if (data.fireBladeSlashUnlocked) return false;
                data.fireBladeSlashUnlocked = true; return true;

            case "fireManaSave":
                if (data.fireManaSave || !data.fireBladeSlashUnlocked) return false;
                data.fireManaSave = true; return true;

            case "fireCritUp":
                if (data.fireCritUp || !data.fireManaSave) return false;
                data.fireCritUp = true; return true;

            case "fireDamageUp":
                if (data.fireDamageUp || !data.fireCritUp) return false;
                data.fireDamageUp = true; return true;

            case "fireBurnDuration":
                if (data.fireBurnDuration || !data.fireDamageUp) return false;
                data.fireBurnDuration = true; return true;

            case "twinInferno":
                if (data.twinInfernoUnlocked || !data.fireBurnDuration) return false;
                data.twinInfernoUnlocked = true; return true;

            default:
                Debug.LogWarning($"[SwordSkillTreeManager] Unknown node: {nodeID}");
                return false;
        }
    }
}
