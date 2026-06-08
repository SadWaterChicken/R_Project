using UnityEngine;
using System.Linq;

/// <summary>
/// Extension methods for Inventory to support weapon mastery tracking
/// </summary>
public static class InventoryExtensions
{
    /// <summary>
    /// Get currently equipped weapon
    /// </summary>
    public static ItemData GetEquippedWeapon(this Inventory inventory)
    {
        return inventory.ownedItems.FirstOrDefault(item => item.equipped && !string.IsNullOrEmpty(item.weaponClassName));
    }

    /// <summary>
    /// Get all weapons of a specific class
    /// </summary>
    public static System.Collections.Generic.List<ItemData> GetWeaponsByClass(this Inventory inventory, string weaponClassName)
    {
        return inventory.ownedItems
            .Where(item => item.weaponClassName == weaponClassName)
            .ToList();
    }

    /// <summary>
    /// Get all forgeable weapons
    /// </summary>
    public static System.Collections.Generic.List<ItemData> GetForgeableWeapons(this Inventory inventory)
    {
        return inventory.ownedItems
            .Where(item => item.isForgeable && !string.IsNullOrEmpty(item.weaponClassName))
            .ToList();
    }

    /// <summary>
    /// Find highest mastery weapon in inventory
    /// </summary>
    public static ItemData GetBestMasteryWeapon(this Inventory inventory)
    {
        return inventory.ownedItems
            .Where(item => !string.IsNullOrEmpty(item.weaponClassName))
            .OrderByDescending(item => item.weaponMastery)
            .FirstOrDefault();
    }
}
