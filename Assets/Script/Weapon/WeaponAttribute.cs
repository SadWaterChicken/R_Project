using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Weapon attributes specific to each weapon class/type
/// Based on game design: Greatsword, Katana, Warhammer, Greatsaxe, Spear, Bow, Staff, Orb
/// </summary>
[System.Serializable]
public class WeaponAttribute
{
    [System.Serializable]
    public enum WeaponType
    {
        Greatsword,   // Heavy sword - Commit, finisher
        Katana,       // Long sword - React, parry/dodge, counter
        Warhammer,    // Impact melee - Windup, control, impact turns
        Greatsaxe,    // Brutal melee - Greed, riposte, cashout usage
        Spear,        // Polearm - Space, poke, reset, footwork
        Bow,          // Ranged - Align, fire, positioning, spacing
        Staff,        // Magic caster - Cast, route, burst, combo
        Orb           // Trick weapon - Hybrid, stance rotate, element interaction
    }

    public WeaponType weaponType;
    public string lootType;                  // Heavy, Short, Long, Impact, Brutal, Polearm, Ranged, Magic caster

    // Gameplay Loop characteristics
    public string gameplayLoop;

    // Combat Role
    public string combatRole;
    public int skillCeiling = 5;             // 1-5 skill ceiling
    public int combatDifficulty = 5;         // 1-5 difficulty

    // Resource & Gameplay
    public string resourceGauge;             // Charge, Tempo, Focus, Impact, Blood Debt, Tipper Loop, Focus Rhythm, Mana, Sigil
    public string countplayDifficulty;       // Counterplay description
    public string coreFeature;               // Special core mechanic

    // Combat abilities
    public List<CombatAction> primaryActions = new List<CombatAction>();
    public List<CombatAction> secondaryActions = new List<CombatAction>();

    [System.Serializable]
    public class CombatAction
    {
        public string name;
        public string description;
        public bool requiresCharge = false;
        public bool requiresCounter = false;
        public float cooldown = 0f;
    }
}

/// <summary>
/// Specific weapon instances with their stats and attributes
/// </summary>
[System.Serializable]
public class WeaponDefinition
{
    public string weaponID;
    public string weaponName;
    public string weaponClassName;
    public WeaponAttribute.WeaponType weaponType;

    // Base stats
    public float baseDamage;
    public float attackSpeed;
    public float range;
    public float critChance;

    // Weapon-specific attributes
    public WeaponAttribute attributes;

    // Scaling
    public float strengthScaling;
    public float dexterityScaling;
    public float intelligenceScaling;

    // Special properties
    public bool canCounter = false;
    public bool canParry = false;
    public bool canCombo = false;
    public bool hasChargeAttack = false;
    public bool hasElementInteraction = false;  // For Orb specifically

    public WeaponDefinition Clone()
    {
        return new WeaponDefinition
        {
            weaponID = this.weaponID,
            weaponName = this.weaponName,
            weaponClassName = this.weaponClassName,
            weaponType = this.weaponType,
            baseDamage = this.baseDamage,
            attackSpeed = this.attackSpeed,
            range = this.range,
            critChance = this.critChance,
            attributes = this.attributes,
            strengthScaling = this.strengthScaling,
            dexterityScaling = this.dexterityScaling,
            intelligenceScaling = this.intelligenceScaling,
            canCounter = this.canCounter,
            canParry = this.canParry,
            canCombo = this.canCombo,
            hasChargeAttack = this.hasChargeAttack,
            hasElementInteraction = this.hasElementInteraction
        };
    }
}

/// <summary>
/// Weapon database - stores all weapon definitions
/// </summary>
public class WeaponDatabase : MonoBehaviour
{
    public static WeaponDatabase Instance { get; private set; }

    [SerializeField] private List<WeaponDefinition> weaponDefinitions = new List<WeaponDefinition>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public WeaponDefinition GetWeaponDefinition(string weaponID)
    {
        foreach (var weapon in weaponDefinitions)
        {
            if (weapon.weaponID == weaponID)
                return weapon.Clone();
        }
        return null;
    }

    public List<WeaponDefinition> GetWeaponsByClass(string weaponClassName)
    {
        var result = new List<WeaponDefinition>();
        foreach (var weapon in weaponDefinitions)
        {
            if (weapon.weaponClassName == weaponClassName)
                result.Add(weapon.Clone());
        }
        return result;
    }

    public List<WeaponDefinition> GetWeaponsByType(WeaponAttribute.WeaponType weaponType)
    {
        var result = new List<WeaponDefinition>();
        foreach (var weapon in weaponDefinitions)
        {
            if (weapon.weaponType == weaponType)
                result.Add(weapon.Clone());
        }
        return result;
    }

    public void AddWeaponDefinition(WeaponDefinition definition)
    {
        if (!weaponDefinitions.Exists(w => w.weaponID == definition.weaponID))
            weaponDefinitions.Add(definition);
    }
}
