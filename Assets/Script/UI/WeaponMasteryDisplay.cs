using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Displays weapon mastery and forging stats in a HUD/Menu
/// Can be placed in a UI panel to always show current weapon info
/// </summary>
public class WeaponMasteryDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Image weaponIcon;
    public TMP_Text weaponNameText;
    public Image masteryFillImage;
    public TMP_Text masteryPercentText;
    public TMP_Text forgeeLevelText;
    public TMP_Text weaponStatsText;

    [Header("Update Rate")]
    [SerializeField] private float updateInterval = 0.5f;
    private float updateTimer;

    private ItemData currentWeapon;

    private void OnEnable()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged += OnWeaponEquipped;
        }
    }

    private void OnDisable()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged -= OnWeaponEquipped;
        }
    }

    private void Update()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            RefreshDisplay();
            updateTimer = updateInterval;
        }
    }

    private void OnWeaponEquipped(ItemData weapon, bool equipped)
    {
        if (equipped && !string.IsNullOrEmpty(weapon.weaponClassName))
        {
            currentWeapon = weapon;
            RefreshDisplay();
        }
        else if (!equipped && currentWeapon == weapon)
        {
            currentWeapon = null;
            ClearDisplay();
        }
    }

    private void RefreshDisplay()
    {
        if (currentWeapon == null)
        {
            ClearDisplay();
            return;
        }

        // Weapon name
        if (weaponNameText != null)
            weaponNameText.text = currentWeapon.itemName;

        // Weapon icon
        if (weaponIcon != null)
        {
            weaponIcon.sprite = string.IsNullOrEmpty(currentWeapon.iconPath)
                ? null
                : Resources.Load<Sprite>(currentWeapon.iconPath);
        }

        // Mastery bar
        float maxMastery = ForgeManager.Instance != null ? ForgeManager.Instance.GetMaxMastery() : 100f;
        if (masteryFillImage != null)
        {
            masteryFillImage.fillAmount = currentWeapon.weaponMastery / maxMastery;
        }

        // Mastery text
        if (masteryPercentText != null)
        {
            masteryPercentText.text = $"{currentWeapon.weaponMastery:F1}/{maxMastery:F0}";
        }

        // Forge level
        if (forgeeLevelText != null)
        {
            int maxLevel = ForgeManager.Instance != null ? ForgeManager.Instance.GetMaxForgeLevel() : 10;
            forgeeLevelText.text = $"Lvl {currentWeapon.forgeLevel}/{maxLevel}";
        }

        // Stats
        if (weaponStatsText != null)
        {
            var statsBuilder = new System.Text.StringBuilder();
            if (currentWeapon.modifiers != null && currentWeapon.modifiers.Count > 0)
            {
                foreach (var mod in currentWeapon.modifiers)
                {
                    var sign = mod.value >= 0 ? "+" : "";
                    var val = mod.percent ? $"{sign}{mod.value}%" : $"{sign}{mod.value}";
                    statsBuilder.AppendLine($"{mod.stat}: {val}");
                }
            }
            weaponStatsText.text = statsBuilder.ToString().TrimEnd();
        }
    }

    private void ClearDisplay()
    {
        if (weaponNameText != null) weaponNameText.text = "---";
        if (weaponIcon != null) weaponIcon.sprite = null;
        if (masteryFillImage != null) masteryFillImage.fillAmount = 0f;
        if (masteryPercentText != null) masteryPercentText.text = "0/100";
        if (forgeeLevelText != null) forgeeLevelText.text = "Lvl 0/10";
        if (weaponStatsText != null) weaponStatsText.text = "";
    }
}
