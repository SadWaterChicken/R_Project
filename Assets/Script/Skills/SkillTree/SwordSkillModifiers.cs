using UnityEngine;

/// <summary>
/// Modifiers từ Sword Skill Tree truyền vào skill khi cast.
/// </summary>
[System.Serializable]
public struct SwordSkillModifiers
{
    // ══ WindSlash modifiers ═══════════════════════════════════════════════════
    /// <summary>ManaSave: giảm 30% mana cost</summary>
    public bool windManaSave;

    /// <summary>RangeUp: tầm 10m → 15m</summary>
    public bool windRangeUp;

    /// <summary>DamageUp: +40% sát thương</summary>
    public bool windDamageUp;

    /// <summary>CooldownDown: cooldown 5s → 3s (áp dụng lúc cast)</summary>
    public bool windCooldownDown;

    /// <summary>TempestBlade: AoE (piercing), +60% dame, +6m tầm thêm</summary>
    public bool tempestBlade;

    // ══ FireBladeSlash modifiers ══════════════════════════════════════════════
    /// <summary>ManaSave: giảm 30% mana cost</summary>
    public bool fireManaSave;

    /// <summary>CritUp: tỉ lệ chí mạng +20%</summary>
    public bool fireCritUp;

    /// <summary>DamageUp: +40% sát thương</summary>
    public bool fireDamageUp;

    /// <summary>BurnDuration: thời gian cháy 3s → 5s</summary>
    public bool fireBurnDuration;

    /// <summary>TwinInferno: tung 2 lần + tăng phạm vi hitbox</summary>
    public bool twinInferno;

    // ─── Default ─────────────────────────────────────────────────────────────
    public static SwordSkillModifiers Default() => new SwordSkillModifiers
    {
        windManaSave     = false,
        windRangeUp      = false,
        windDamageUp     = false,
        windCooldownDown = false,
        tempestBlade     = false,
        fireManaSave     = false,
        fireCritUp       = false,
        fireDamageUp     = false,
        fireBurnDuration = false,
        twinInferno      = false,
    };
}
