using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat Instance { get; private set; }

    [Header("Base Stats")]
    public int maxHealth;
    public float currentHealth;
    public float healthRegenRate;
    public float physicalDamage;
    public float magicDamage;
    public float defense;
    public float sanity;
    public float sanityRegenRate;
    public float mana;
    public float manaRegenRate;
    public int gold = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        // Subscribe to inventory changes every time this becomes active
        // (safer than Start in case Inventory.Instance loads later)
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged += OnItemEquipChanged;
            Debug.Log("[PlayerStat] Subscribed to Inventory.OnItemEquipChanged");
        }
        else
        {
            Debug.LogWarning("[PlayerStat] Inventory.Instance is null in OnEnable!");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged -= OnItemEquipChanged;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged -= OnItemEquipChanged;
        }
    }

    // Called when item is equipped/unequipped
    private void OnItemEquipChanged(ItemData item, bool equipped)
    {
        Debug.Log($"[PlayerStat] OnItemEquipChanged called: {item?.itemName} equipped={equipped}");
        UpdateStatsForItem(item, equipped);
    }

    // Update base stats directly when item is equipped/unequipped
    private void UpdateStatsForItem(ItemData item, bool isEquipped)
    {
        if (item?.modifiers == null || item.modifiers.Count == 0)
            return;

        float multiplier = isEquipped ? 1f : -1f; // +1 for equip, -1 for unequip

        foreach (var mod in item.modifiers)
        {
            float value = mod.value * multiplier;
            Debug.Log($"[PlayerStat] {(isEquipped ? "Equipping" : "Unequipping")} {item.itemName}: {mod.stat} {(isEquipped ? "+" : "-")}{Mathf.Abs(mod.value)}");

            switch (mod.stat.ToLower())
            {
                case "physical damage":
                case "physicaldamage":
                    physicalDamage += value;
                    break;
                case "magic damage":
                case "magicdamage":
                    magicDamage += value;
                    break;
                case "defense":
                    defense += value;
                    break;
                case "max health":
                case "maxhealth":
                    maxHealth += (int)value;
                    break;
                default:
                    Debug.LogWarning($"[PlayerStat] Unknown stat type: {mod.stat}");
                    break;
            }
        }

        Debug.Log($"[PlayerStat] Stats updated - PhysicalDmg: {physicalDamage}, MagicDmg: {magicDamage}, Defense: {defense}");
    }

    // Get final stat values (base stats are already updated by equipping items)
    public float GetPhysicalDamage()
    {
        return physicalDamage;
    }

    public float GetMagicDamage()
    {
        return magicDamage;
    }

    public float GetDefense()
    {
        return defense;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle player death (e.g., play animation, disable controls, etc.)
        Debug.Log("Player has died.");
    }

    public int GetGold()
    {
        return gold;
    }

    public void AddGold(int amount)
    {
        if (amount > 0)
            gold += amount;
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        return true;
    }
}
