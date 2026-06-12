using UnityEngine;
using System.Collections.Generic;

public class CharacterBuffManager : MonoBehaviour
{
    private Dictionary<DungeonBuff.BuffType, float> activeBuffs = new Dictionary<DungeonBuff.BuffType, float>();
    private Dictionary<DungeonBuff.BuffType, int> buffStacks = new Dictionary<DungeonBuff.BuffType, int>();

    // ApplyBuff: add or stack a buff, enforce stacking rules, and raise event
    public void ApplyBuff(DungeonBuff buff)
    {
        if (buff == null)
        {
            Debug.LogWarning("[CharacterBuffManager] Trying to apply null buff!");
            return;
        }

        // Check if stackable
        if (!buff.isStackable && activeBuffs.ContainsKey(buff.type))
        {
            Debug.Log($"[CharacterBuffManager] Buff {buff.buffName} is not stackable and already active!");
            return;
        }

        // Check max stacks
        if (buff.isStackable)
        {
            if (!buffStacks.ContainsKey(buff.type))
                buffStacks[buff.type] = 0;

            if (buffStacks[buff.type] >= buff.maxStacks)
            {
                Debug.Log($"[CharacterBuffManager] Buff {buff.buffName} reached max stacks ({buff.maxStacks})!");
                return;
            }

            buffStacks[buff.type]++;
        }

        // Apply buff value
        if (!activeBuffs.ContainsKey(buff.type))
            activeBuffs[buff.type] = 0f;

        activeBuffs[buff.type] += buff.value;

        Debug.Log($"[CharacterBuffManager] Applied buff: {buff.buffName} (Type: {buff.type}, Value: +{buff.value * 100}%)");
        Debug.Log($"[CharacterBuffManager] Total {buff.type}: {activeBuffs[buff.type] * 100}%");

        // Raise event for UI updates
        DungeonEvents.RaiseBuffApplied(buff);
    }

    // GetBuffValue: returns total active value for a buff type
    public float GetBuffValue(DungeonBuff.BuffType type)
    {
        return activeBuffs.ContainsKey(type) ? activeBuffs[type] : 0f;
    }

    // GetBuffStacks: returns current stack count for a buff type
    public int GetBuffStacks(DungeonBuff.BuffType type)
    {
        return buffStacks.ContainsKey(type) ? buffStacks[type] : 0;
    }

    // ClearAllBuffs: remove all active buffs and stacks
    public void ClearAllBuffs()
    {
        Debug.Log("[CharacterBuffManager] Clearing all buffs!");
        activeBuffs.Clear();
        buffStacks.Clear();
    }

    // GetAllActiveBuffs: returns a copy of all active buff values
    public Dictionary<DungeonBuff.BuffType, float> GetAllActiveBuffs()
    {
        return new Dictionary<DungeonBuff.BuffType, float>(activeBuffs);
    }
}
