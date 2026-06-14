using UnityEngine;
using System.Collections.Generic;

public class EquipmentStatsViewer : MonoBehaviour
{
    [Header("Health & Mana Stats")]
    public int maxHealthBonus;
    public int maxManaBonus;
    public float healthRegenRateBonus;

    [Header("Damage Stats")]
    public float physicalDamageBonus;
    public float magicDamageBonus;

    [Header("Defence Stats")]
    public float physicalArmorBonus;
    public float magicArmorBonus;

    [Header("Movement & Combat")]
    public float movementSpeedBonus;
    public float attackSpeedBonus;

    [Header("Luck & Crit")]
    public float critChanceBonus;
    public float luckBonus;

    [Header("Shield Stats")]
    public float maxShieldBonus;
    public float shieldRegenRateBonus;

    private void OnEnable()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged += OnItemEquipChanged;
        }
    }

    private void OnDisable()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged -= OnItemEquipChanged;
        }
    }

    private void Start()
    {
        // Khi vừa mở game lên, quét qua túi đồ tìm đồ đang mặc để cộng chỉ số vào ngay lập tức
        if (Inventory.Instance != null)
        {
            foreach (var item in Inventory.Instance.ownedItems)
            {
                if (item.equipped)
                {
                    OnItemEquipChanged(item, true);
                }
            }
        }
    }

    private void OnItemEquipChanged(ItemData item, bool isEquipped)
    {
        if (item?.modifiers == null || item.modifiers.Count == 0)
            return;

        float multiplier = isEquipped ? 1f : -1f;

        foreach (var mod in item.modifiers)
        {
            float value = mod.value * multiplier;

            switch (mod.stat.ToLower())
            {
                case "physical damage":
                case "physicaldamage":
                case "physical damage bonus":
                case "physicaldamagebonus":
                    physicalDamageBonus += value;
                    break;
                case "magic damage":
                case "magicdamage":
                case "magic damage bonus":
                case "magicdamagebonus":
                    magicDamageBonus += value;
                    break;
                case "physical armour":
                case "physicalarmour":
                case "physical armor":
                case "physicalarmor":
                    physicalArmorBonus += value;
                    break;
                case "magic armour":
                case "magicarmour":
                case "magic armor":
                case "magicarmor":
                    magicArmorBonus += value;
                    break;
                case "max health":
                case "maxhealth":
                    maxHealthBonus += (int)value;
                    break;
                case "max mana":
                case "maxmana":
                    maxManaBonus += (int)value;
                    break;
                case "max shield":
                case "maxshield":
                    maxShieldBonus += value;
                    break;
                case "movement speed":
                case "movementspeed":
                    movementSpeedBonus += value;
                    break;
                case "attack speed":
                case "attackspeed":
                    attackSpeedBonus += value;
                    break;
                case "crit chance":
                case "critchance":
                    critChanceBonus += value;
                    break;
                case "luck":
                    luckBonus += value;
                    break;
            }
        }
    }
}
