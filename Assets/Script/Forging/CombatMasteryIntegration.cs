using UnityEngine;

/// <summary>
/// Integrates weapon mastery gains into combat system
/// Hook this into EnemyStat.Die() or whenever an enemy is defeated
/// </summary>
public class CombatMasteryIntegration : MonoBehaviour
{
    public static void OnEnemyDefeated()
    {
        var inventory = Inventory.Instance;
        if (inventory == null) return;

        // Get equipped weapon
        var equippedWeapon = inventory.GetEquippedWeapon();
        if (equippedWeapon == null) return;

        // Add mastery
        var forgeManager = ForgeManager.Instance;
        if (forgeManager != null)
        {
            forgeManager.AddMasteryOnKill(equippedWeapon);
        }
    }
}
