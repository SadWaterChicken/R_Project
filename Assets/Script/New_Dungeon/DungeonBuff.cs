using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Buff", fileName = "New Buff")]
public class DungeonBuff : ScriptableObject
{
    public enum BuffType
    {
        DropRate,
        GoldMultiplier,
        DamageBoost,
        SpeedBoost,
        Lifesteal,
        CritChance,
        ShieldOnKill,
        ExpBoost,
        HealthRegen,
        ManaRegen
    }

    public string buffName = "New Buff";
    public string description = "Buff description";
    public Sprite icon;
    
    public BuffType type = BuffType.DamageBoost;
    public float value = 0.25f; // 0.25 = +25%
    
    public bool isStackable = true;
    public int maxStacks = 3;

    // ToString: returns a short human-readable description of the buff
    public override string ToString()
    {
        return $"{buffName} ({type}): +{value * 100}%";
    }
}
