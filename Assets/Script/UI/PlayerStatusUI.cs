using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("Health & Mana Texts")]
    public TMP_Text maxHealthText;
    public TMP_Text currentHealthText;
    public TMP_Text healthRegenText;
    public TMP_Text maxManaText;
    public TMP_Text currentManaText;
    public TMP_Text manaRegenText;

    [Header("Health & Mana Sliders")]
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider sanitySlider;

    [Header("Damage Stats")]
    public TMP_Text basePhysicalDamageText;
    public TMP_Text physicalDamageBonusText;
    public TMP_Text baseMagicDamageText;
    public TMP_Text magicDamageBonusText;

    [Header("Defence & Shield Texts")]
    public TMP_Text physicalArmorText;
    public TMP_Text magicArmorText;
    public TMP_Text shieldText;
    public TMP_Text maxShieldText;
    public TMP_Text shieldRegenText;
    public TMP_Text shieldCooldownText;

    [Header("Shield Slider")]
    public Slider shieldSlider;

    [Header("Movement & Combat")]
    public TMP_Text baseSpeedText;
    public TMP_Text movementSpeedText;
    public TMP_Text attackSpeedText;
    public TMP_Text critChanceText;
    public TMP_Text dashCooldownText;
    public TMP_Text isInvincibleText;

    [Header("Economy & Sanity")]
    public TMP_Text goldText;
    public TMP_Text energyCubesText;
    public TMP_Text currentSanityText;
    public TMP_Text maxSanityText;
    public TMP_Text luckText;

    [Header("Weapon Class Masteries (Texts)")]
    public TMP_Text greatswordMasteryText;
    public TMP_Text katanaMasteryText;
    public TMP_Text warhammerMasteryText;
    public TMP_Text greataxeMasteryText;
    public TMP_Text spearMasteryText;
    public TMP_Text bowMasteryText;
    public TMP_Text staffMasteryText;
    public TMP_Text orbMasteryText;

    [Header("Weapon Class Masteries (Sliders)")]
    public Slider greatswordSlider;
    public Slider katanaSlider;
    public Slider warhammerSlider;
    public Slider greataxeSlider;
    public Slider spearSlider;
    public Slider bowSlider;
    public Slider staffSlider;
    public Slider orbSlider;

    private float updateTimer;

    private void OnEnable()
    {
        UpdateStatusUI();
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= 0.1f)
        {
            updateTimer = 0f;
            UpdateStatusUI();
        }
    }

    public void UpdateStatusUI()
    {
        if (PlayerStat.Instance == null) return;

        var ps = PlayerStat.Instance;
        string vColor = "<color=#FFD700>";
        string endC = "</color>";

        // --- TEXTS ---
        if (maxHealthText != null) maxHealthText.text = $"Max Health: {vColor}{ps.maxHealth}{endC}";
        if (currentHealthText != null) currentHealthText.text = $"{ps.currentHealth:F0} / {ps.maxHealth}";
        if (healthRegenText != null) healthRegenText.text = $"Health Regen: {vColor}{ps.healthRegenRate:F1}{endC}";
        
        if (maxManaText != null) maxManaText.text = $"Max Mana: {vColor}{ps.maxMana}{endC}";
        if (currentManaText != null) currentManaText.text = $"{ps.currentMana:F0} / {ps.maxMana}";
        if (manaRegenText != null) manaRegenText.text = $"Mana Regen: {vColor}{ps.manaRegenRate:F1}{endC}";

        if (basePhysicalDamageText != null) basePhysicalDamageText.text = $"Base Phys DMG: {vColor}{ps.basePhysicalDamage}{endC}";
        if (physicalDamageBonusText != null) physicalDamageBonusText.text = $"Phys Bonus: {vColor}{ps.physicalDamageBonus}{endC}";
        if (baseMagicDamageText != null) baseMagicDamageText.text = $"Base Magic DMG: {vColor}{ps.baseMagicDamage}{endC}";
        if (magicDamageBonusText != null) magicDamageBonusText.text = $"Magic Bonus: {vColor}{ps.magicDamageBonus}{endC}";

        if (physicalArmorText != null) physicalArmorText.text = $"Physical Armor: {vColor}{ps.physicalArmor}{endC}";
        if (magicArmorText != null) magicArmorText.text = $"Magic Armor: {vColor}{ps.magicArmor}{endC}";
        if (shieldText != null) shieldText.text = $"{ps.shield:F0} / {ps.maxShield}";
        if (maxShieldText != null) maxShieldText.text = $"Max Shield: {vColor}{ps.maxShield}{endC}";
        if (shieldRegenText != null) shieldRegenText.text = $"Shield Regen: {vColor}{ps.shieldRegenRate:F1}{endC}";
        if (shieldCooldownText != null) shieldCooldownText.text = $"Shield CD: {vColor}{ps.shieldRechargeCooldown:F1}{endC}";

        if (baseSpeedText != null) baseSpeedText.text = $"Base Speed: {vColor}{ps.baseSpeed:F1}{endC}";
        if (movementSpeedText != null) movementSpeedText.text = $"Move Speed: {vColor}{ps.movementSpeed:F1}{endC}";
        if (attackSpeedText != null) attackSpeedText.text = $"Attack Speed: {vColor}{ps.attackSpeed:F1}{endC}";
        if (critChanceText != null) critChanceText.text = $"Crit Chance: {vColor}{ps.critChance:F1}{endC}";
        if (dashCooldownText != null) dashCooldownText.text = $"Dash CD: {vColor}{ps.dashCooldown:F1}{endC}";
        if (isInvincibleText != null) isInvincibleText.text = $"Invincible: {vColor}{(ps.isInvincible ? "Yes" : "No")}{endC}";

        if (goldText != null) goldText.text = $"Gold: {vColor}{ps.gold}{endC}";
        if (energyCubesText != null) energyCubesText.text = $"Energy: {vColor}{ps.currentEnergyCubes}{endC}";
        if (currentSanityText != null) currentSanityText.text = $"{ps.currentSanity:F0} / {ps.maxSanity}";
        if (maxSanityText != null) maxSanityText.text = $"Max Sanity: {vColor}{ps.maxSanity}{endC}";
        if (luckText != null) luckText.text = $"Luck: {vColor}{ps.luck:F1}{endC}";

        if (greatswordMasteryText != null) greatswordMasteryText.text = $"{ps.greatswordMastery:F1}";
        if (katanaMasteryText != null) katanaMasteryText.text = $"{ps.katanaMastery:F1}";
        if (warhammerMasteryText != null) warhammerMasteryText.text = $"{ps.warhammerMastery:F1}";
        if (greataxeMasteryText != null) greataxeMasteryText.text = $"{ps.greatsaxeMastery:F1}";
        if (spearMasteryText != null) spearMasteryText.text = $"{ps.spearMastery:F1}";
        if (bowMasteryText != null) bowMasteryText.text = $"{ps.bowMastery:F1}";
        if (staffMasteryText != null) staffMasteryText.text = $"{ps.staffMastery:F1}";
        if (orbMasteryText != null) orbMasteryText.text = $"{ps.orbMastery:F1}";

        // --- SLIDERS ---
        if (healthSlider != null) { healthSlider.maxValue = ps.maxHealth; healthSlider.value = ps.currentHealth; }
        if (manaSlider != null) { manaSlider.maxValue = ps.maxMana; manaSlider.value = ps.currentMana; }
        if (sanitySlider != null) { sanitySlider.maxValue = ps.maxSanity; sanitySlider.value = ps.currentSanity; }
        if (shieldSlider != null) { shieldSlider.maxValue = ps.maxShield > 0 ? ps.maxShield : 1; shieldSlider.value = ps.shield; }

        float maxMastery = 100f; // Giả sử mastery tối đa là 100
        if (greatswordSlider != null) { greatswordSlider.maxValue = maxMastery; greatswordSlider.value = ps.greatswordMastery; }
        if (katanaSlider != null) { katanaSlider.maxValue = maxMastery; katanaSlider.value = ps.katanaMastery; }
        if (warhammerSlider != null) { warhammerSlider.maxValue = maxMastery; warhammerSlider.value = ps.warhammerMastery; }
        if (greataxeSlider != null) { greataxeSlider.maxValue = maxMastery; greataxeSlider.value = ps.greatsaxeMastery; }
        if (spearSlider != null) { spearSlider.maxValue = maxMastery; spearSlider.value = ps.spearMastery; }
        if (bowSlider != null) { bowSlider.maxValue = maxMastery; bowSlider.value = ps.bowMastery; }
        if (staffSlider != null) { staffSlider.maxValue = maxMastery; staffSlider.value = ps.staffMastery; }
        if (orbSlider != null) { orbSlider.maxValue = maxMastery; orbSlider.value = ps.orbMastery; }
    }
}
