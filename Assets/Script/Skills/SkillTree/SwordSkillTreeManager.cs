using UnityEngine;

/// <summary>
/// Singleton quản lý Sword Skill Tree — cây mới 2 nhánh:
///   Root → WindSlash → [CritDmg → FireBlade → ..] và [ManaSave → .. → TempestBlade]
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
    public bool IsTempestBladeUnlocked()     => data.tempestBladeUnlocked;
    public bool IsTwinInfernoUnlocked()      => data.twinInfernoUnlocked;

    /// <summary>Kiểm tra 1 node có thể unlock không (đủ SP + đủ prereq + chưa mở)</summary>
    public bool CanUnlockNode(string nodeID)
    {
        int cost = SwordSkillTreeData.GetNodeCost(nodeID);
        if (data.availableSkillPoints < cost) return false;
        return CheckPrereq(nodeID) && !IsNodeUnlocked(nodeID);
    }

    /// <summary>Kiểm tra node đã mở chưa</summary>
    public bool IsNodeUnlocked(string nodeID)
    {
        if (string.IsNullOrEmpty(nodeID)) return false;
        switch (nodeID.Trim())
        {
            case "baseDamageUp":      return data.baseDamageUp;
            case "windSlash":         return data.windSlashUnlocked;
            case "windCritDamageUp":  return data.windCritDamageUp;
            case "windManaSave":      return data.windManaSave;
            case "windDamageUp":      return data.windDamageUp;
            case "windRangeUp":       return data.windRangeUp;
            case "windCooldownDown":  return data.windCooldownDown;
            case "tempestBlade":      return data.tempestBladeUnlocked;
            case "fireBladeSlash":    return data.fireBladeSlashUnlocked;
            case "fireManaSave":      return data.fireManaSave;
            case "fireCritUp":        return data.fireCritUp;
            case "fireDamageUp":      return data.fireDamageUp;
            case "fireBurnDuration":  return data.fireBurnDuration;
            case "twinInferno":       return data.twinInfernoUnlocked;
            default:                  return false;
        }
    }

    /// <summary>Kiểm tra prereq có thỏa mãn không (không cần SP)</summary>
    public bool CheckPrereq(string nodeID)
    {
        if (string.IsNullOrEmpty(nodeID)) return false;
        switch (nodeID.Trim())
        {
            case "baseDamageUp":      return true;
            case "windSlash":         return data.baseDamageUp;
            case "windCritDamageUp":  return data.windSlashUnlocked;
            case "windManaSave":      return data.windSlashUnlocked;
            case "windDamageUp":      return data.windManaSave;
            case "windRangeUp":       return data.windDamageUp;
            case "windCooldownDown":  return data.windRangeUp;
            case "tempestBlade":      return data.windCooldownDown;
            case "fireBladeSlash":    return data.windCritDamageUp;
            case "fireManaSave":      return data.fireBladeSlashUnlocked;
            case "fireCritUp":        return data.fireManaSave;
            case "fireDamageUp":      return data.fireCritUp;
            case "fireBurnDuration":  return data.fireDamageUp;
            case "twinInferno":       return data.fireBurnDuration;
            default:                  return false;
        }
    }

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
        // Root
        baseDamageUp     = data.baseDamageUp,
        // WindSlash
        windCritDamageUp = data.windCritDamageUp,
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
    public void ApplyWindSlashMods(WindSlashSkill slash, SwordSkillModifiers mods)
    {
        if (slash == null) return;

        // BaseDamageUp: +20% cho tất cả Greatsword skills
        if (mods.baseDamageUp)
            slash.fallbackDamageMultiplier *= 1.2f;

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
            slash.hitboxSize = new Vector3(6f, 2f, 2f);
        }
    }

    // ─── APPLY FIRE MODS AT RUNTIME ──────────────────────────────────────────
    public void ApplyFireBladeMods(FireBladeSlashSkill fire, SwordSkillModifiers mods)
    {
        if (fire == null) return;

        // BaseDamageUp: +20%
        if (mods.baseDamageUp)
        {
            // damageMultiplier is protected in BaseSkill — apply via reflection-safe approach
            // FireBladeSlashSkill reads CalculatePhysicalDamage() which uses damageMultiplier
            // We set it directly since we have access at cast time through ApplyFireBladeMods
            var field = typeof(BaseSkill).GetField("damageMultiplier",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                float current = (float)field.GetValue(fire);
                field.SetValue(fire, current * 1.2f);
            }
        }

        // BurnDuration: 3s → 5s
        if (mods.fireBurnDuration)
            fire.baseDotDuration = 5f;

        // TwinInferno: mở rộng hitbox
        if (mods.twinInferno)
            fire.baseHitboxSize = new Vector3(fire.baseHitboxSize.x * 1.5f,
                                              fire.baseHitboxSize.y,
                                              fire.baseHitboxSize.z);
    }

    /// <summary>
    /// Reset toàn bộ cây về trạng thái chưa mở khoá.
    /// Hoàn lại đúng bằng số SP đã tiêu vào các node, giữ nguyên totalSpGranted.
    /// </summary>
    public void ResetAllNodes()
    {
        // Tính SP đã tiêu để hoàn lại
        int refund = 0;
        string[] allNodes = {
            "baseDamageUp", "windSlash", "windCritDamageUp", "windManaSave",
            "windDamageUp", "windRangeUp", "windCooldownDown", "tempestBlade",
            "fireBladeSlash", "fireManaSave", "fireCritUp",
            "fireDamageUp", "fireBurnDuration", "twinInferno"
        };
        foreach (var nodeID in allNodes)
        {
            if (IsNodeUnlocked(nodeID))
                refund += SwordSkillTreeData.GetNodeCost(nodeID);
        }

        // Reset toàn bộ node về false
        data.baseDamageUp          = false;
        data.windSlashUnlocked      = false;
        data.windCritDamageUp       = false;
        data.windManaSave            = false;
        data.windDamageUp            = false;
        data.windRangeUp             = false;
        data.windCooldownDown        = false;
        data.tempestBladeUnlocked    = false;
        data.fireBladeSlashUnlocked  = false;
        data.fireManaSave            = false;
        data.fireCritUp              = false;
        data.fireDamageUp            = false;
        data.fireBurnDuration        = false;
        data.twinInfernoUnlocked     = false;

        // Hoàn lại SP đã tiêu
        data.availableSkillPoints += refund;

        data.Save();
        Debug.Log($"[SwordSkillTreeManager] Reset xong — hoàn lại {refund} SP | Có sẵn: {data.availableSkillPoints} SP");
    }

    /// <summary>Xoá sạch toàn bộ EXP, SP, trả cây về trạng thái khởi thủy (0 EXP, 0 SP)</summary>
    public void HardReset()
    {
        data = new SwordSkillTreeData();
        data.Save();

        if (SwordMasteryTracker.Instance != null)
        {
            SwordMasteryTracker.Instance.ResetExp();
        }

        Debug.Log("[SwordSkillTreeManager] HARD RESET thành công! Toàn bộ điểm đã về 0.");
    }

    // ─── UNLOCK LOGIC ────────────────────────────────────────────────────────
    private bool ApplyUnlock(string nodeID)
    {
        if (string.IsNullOrEmpty(nodeID)) return false;
        nodeID = nodeID.Trim();
        
        if (IsNodeUnlocked(nodeID)) return false;
        if (!CheckPrereq(nodeID))  return false;

        switch (nodeID)
        {
            case "baseDamageUp":      data.baseDamageUp          = true; break;
            case "windSlash":         data.windSlashUnlocked      = true; break;
            case "windCritDamageUp":  data.windCritDamageUp       = true; break;
            case "windManaSave":      data.windManaSave            = true; break;
            case "windDamageUp":      data.windDamageUp            = true; break;
            case "windRangeUp":       data.windRangeUp             = true; break;
            case "windCooldownDown":  data.windCooldownDown        = true; break;
            case "tempestBlade":      data.tempestBladeUnlocked    = true; break;
            case "fireBladeSlash":    data.fireBladeSlashUnlocked  = true; break;
            case "fireManaSave":      data.fireManaSave            = true; break;
            case "fireCritUp":        data.fireCritUp              = true; break;
            case "fireDamageUp":      data.fireDamageUp            = true; break;
            case "fireBurnDuration":  data.fireBurnDuration        = true; break;
            case "twinInferno":       data.twinInfernoUnlocked     = true; break;
            default:
                Debug.LogWarning($"[SwordSkillTreeManager] Unknown node: {nodeID}");
                return false;
        }
        return true;
    }
}
