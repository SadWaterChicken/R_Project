using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// UI for the Forging/Smithing NPC
/// Displays weapons that can be forged and weapon mastery information
/// </summary>
public class ForgeUI : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject forgePanel;
    public TMP_Text npcNameText;
    public Button closeButton;

    [Header("Weapon List")]
    public Transform weaponListParent;
    public GameObject weaponSlotPrefab;
    private List<GameObject> spawnedWeaponSlots = new List<GameObject>();

    [Header("Weapon Detail Panel")]
    public GameObject detailPanel;
    public Image weaponIcon;
    public TMP_Text weaponNameText;
    public TMP_Text weaponDescText;
    public TMP_Text weaponClassText;
    public Slider masteryProgressBar;      // Unity standard Slider for mastery bar
    public TMP_Text masteryPercentText;
    public TMP_Text forgeeLevelText;

    [Header("Result Preview (New UI)")]
    public GameObject resultPreviewGroup; // Assign the parent object of the arrow and result icon
    public Image resultWeaponIcon;
    public TMP_Text resultWeaponNameText;

    [Header("Recipe Book Popup")]
    public GameObject recipeBookPanel;
    public Button openRecipeBookButton;
    public Button closeRecipeBookButton;
    public Transform recipeBookContentParent;
    public GameObject recipeBookSlotPrefab;

    [Header("Advanced Weapons Preview")]
    public GameObject advancedWeaponsPanel;
    public Transform advancedWeaponsContentParent;
    public Button closeAdvancedWeaponsButton;
    private List<GameObject> spawnedAdvancedWeaponSlots = new List<GameObject>();

    [Header("Stats Display")]
    public TMP_Text statsText;

    [Header("Forging Requirements")]
    public Transform materialsListParent;
    public GameObject materialSlotPrefab;
    private List<GameObject> spawnedMaterialSlots = new List<GameObject>();

    public TMP_Text goldRequiredText;
    public TMP_Text weaponsNeededText;
    public Button forgeButton;

    private ItemData selectedWeapon;
    private ForgingRecipe currentRecipe;

    private void Awake()
    {
        forgePanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
        if (advancedWeaponsPanel != null) advancedWeaponsPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged += RefreshWeaponList;
        if (ForgingSystem.Instance != null)
            ForgingSystem.Instance.OnMaterialInventoryChanged += RefreshMaterialRequirements;
            
        CursorManager.OnCloseAllUI += CloseForgeUI;
    }

    private void OnDisable()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged -= RefreshWeaponList;
        if (ForgingSystem.Instance != null)
            ForgingSystem.Instance.OnMaterialInventoryChanged -= RefreshMaterialRequirements;
            
        CursorManager.OnCloseAllUI -= CloseForgeUI;
    }

    public void Init(string npcName)
    {
        if (npcNameText != null) 
            npcNameText.text = npcName;

        // Clear placeholder "New Text" values
        if (weaponNameText != null) weaponNameText.text = string.Empty;
        if (resultWeaponNameText != null) resultWeaponNameText.text = string.Empty;
        if (weaponDescText != null) weaponDescText.text = string.Empty;
        if (statsText != null) statsText.text = string.Empty;
        if (resultPreviewGroup != null) resultPreviewGroup.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
        if (recipeBookPanel != null) recipeBookPanel.SetActive(false);
        if (advancedWeaponsPanel != null) advancedWeaponsPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseForgeUI);
        }

        if (forgeButton != null)
        {
            forgeButton.onClick.RemoveAllListeners();
            forgeButton.onClick.AddListener(OnForgeButtonClicked);
        }

        if (openRecipeBookButton != null)
        {
            openRecipeBookButton.onClick.RemoveAllListeners();
            openRecipeBookButton.onClick.AddListener(OpenRecipeBook);
        }

        if (closeRecipeBookButton != null)
        {
            closeRecipeBookButton.onClick.RemoveAllListeners();
            closeRecipeBookButton.onClick.AddListener(() => { if (recipeBookPanel != null) recipeBookPanel.SetActive(false); });
        }

        if (closeAdvancedWeaponsButton != null)
        {
            closeAdvancedWeaponsButton.onClick.RemoveAllListeners();
            closeAdvancedWeaponsButton.onClick.AddListener(() => { if (advancedWeaponsPanel != null) advancedWeaponsPanel.SetActive(false); });
        }

        if (resultWeaponIcon != null)
        {
            Button btn = resultWeaponIcon.GetComponent<Button>();
            if (btn == null) btn = resultWeaponIcon.gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                if (selectedWeapon != null) ShowAdvancedWeapons(selectedWeapon);
            });
        }

        forgePanel.SetActive(true);
        if (CursorManager.Instance != null) CursorManager.Instance.SetUIOpen(true);
        RefreshWeaponList();
    }

    /// <summary>
    /// Refresh the list of weapons player owns that can be forged
    /// </summary>
    private void RefreshWeaponList()
    {
        // Clear existing slots
        foreach (var slot in spawnedWeaponSlots)
            Destroy(slot);
        spawnedWeaponSlots.Clear();

        if (Inventory.Instance == null) return;

        // Filter forgeable weapons (Weapons that have at least one recipe asking for them as a base weapon)
        var forgeableWeapons = Inventory.Instance.ownedItems
            .Where(w => !string.IsNullOrEmpty(w.weaponClassName) && 
                        ForgingSystem.Instance != null && 
                        ForgingSystem.Instance.recipes.Any(r => r.requiredWeapons != null && r.requiredWeapons.Any(reqW => reqW.weapon != null && reqW.weapon.itemID == w.itemID)))
            .ToList();

        foreach (var weapon in forgeableWeapons)
        {
            GameObject slot = Instantiate(weaponSlotPrefab, weaponListParent);
            spawnedWeaponSlots.Add(slot);

            var slotUI = slot.GetComponent<WeaponSlotUI>();
            if (slotUI != null)
            {
                slotUI.SetWeapon(weapon, () => ShowWeaponDetail(weapon));
            }
        }
    }

    /// <summary>
    /// Show detailed information about selected weapon
    /// </summary>
    private void ShowWeaponDetail(ItemData weapon)
    {
        selectedWeapon = weapon;
        detailPanel.SetActive(true);

        // Hide old weapon icon and name
        if (weaponIcon != null) weaponIcon.gameObject.SetActive(false);
        if (weaponNameText != null) weaponNameText.gameObject.SetActive(false);
        
        if (weaponDescText != null) weaponDescText.text = weapon.description;
        if (weaponClassText != null) weaponClassText.text = $"Class: {weapon.weaponClassName}";
        if (forgeeLevelText != null) forgeeLevelText.text = $"Forge Level: {weapon.forgeLevel}/{ForgeManager.Instance.GetMaxForgeLevel()}";

        // Mastery display
        UpdateMasteryDisplay(weapon);

        // Stats
        // Also update recipe if needed
        if (currentRecipe == null || (currentRecipe.requiredWeapons != null && !currentRecipe.requiredWeapons.Any(reqW => reqW.weapon != null && reqW.weapon.itemID == weapon.itemID)))
        {
            currentRecipe = ForgingSystem.Instance?.recipes.FirstOrDefault(r => 
            r.requiredWeapons != null && r.requiredWeapons.Any(reqW => reqW.weapon != null && reqW.weapon.itemID == weapon.itemID));
        }  ItemData resultWeaponPreview = null;

        if (currentRecipe != null)
        {
            RefreshMaterialRequirements();
            
            // Generate a preview of the result weapon
            resultWeaponPreview = ForgeManager.Instance?.GetPreviewWeapon(currentRecipe);
            if (resultWeaponPreview != null)
            {
                if (resultPreviewGroup != null) resultPreviewGroup.SetActive(true);
                if (resultWeaponIcon != null) resultWeaponIcon.sprite = string.IsNullOrEmpty(resultWeaponPreview.iconPath) ? null : Resources.Load<Sprite>(resultWeaponPreview.iconPath);
                if (resultWeaponNameText != null) resultWeaponNameText.text = resultWeaponPreview.itemName;
            }
            else
            {
                if (resultPreviewGroup != null) resultPreviewGroup.SetActive(false);
            }
        }
        else
        {
            // Clear requirements panel — no recipe for this weapon
            foreach (var slot in spawnedMaterialSlots) Destroy(slot);
            spawnedMaterialSlots.Clear();
            if (goldRequiredText != null) goldRequiredText.text = "No recipe available";
            if (weaponsNeededText != null) weaponsNeededText.gameObject.SetActive(false);
            if (forgeButton != null) forgeButton.interactable = false;
            
            if (resultPreviewGroup != null) resultPreviewGroup.SetActive(false);
        }

        // Stats
        UpdateStatsDisplay(weapon, resultWeaponPreview);
    }

    /// <summary>
    /// Update mastery progress bar and text
    /// </summary>
    private void UpdateMasteryDisplay(ItemData weapon)
    {
        float currentMastery = 0f;
        if (Inventory.Instance != null && !string.IsNullOrEmpty(weapon.weaponClassName))
        {
            currentMastery = Inventory.Instance.GetClassMastery(weapon.weaponClassName);
        }

        float masteryPercent = currentMastery / ForgeManager.Instance.GetMaxMastery() * 100f;

        if (masteryProgressBar != null)
        {
            masteryProgressBar.maxValue = ForgeManager.Instance.GetMaxMastery();
            masteryProgressBar.value = currentMastery;
        }

        if (masteryPercentText != null)
        {
            masteryPercentText.text = $"Mastery: {currentMastery:F1}/{ForgeManager.Instance.GetMaxMastery():F0}";
        }
    }

    /// <summary>
    /// Display weapon stats and comparison if preview is available
    /// </summary>
    private void UpdateStatsDisplay(ItemData weapon, ItemData resultWeapon = null)
    {
        if (statsText == null) return;

        var sb = new System.Text.StringBuilder();
        if (weapon.modifiers != null && weapon.modifiers.Count > 0)
        {
            // 1. In ra Dòng Chính trước (Màu Vàng / Cam)
            foreach (var mod in weapon.modifiers)
            {
                if (!mod.isMainStat) continue;

                var sign = mod.value >= 0 ? "+" : "";
                var val = mod.percent ? $"{sign}{mod.value}%" : $"{sign}{mod.value}";
                
                // If we have a preview, look for the same stat to show the difference
                if (resultWeapon != null && resultWeapon.modifiers != null)
                {
                    var resultMod = resultWeapon.modifiers.FirstOrDefault(m => m.stat == mod.stat && m.isMainStat);
                    if (resultMod != null)
                    {
                        float diff = resultMod.value - mod.value;
                        if (diff > 0)
                        {
                            var diffVal = resultMod.percent ? $"+{diff}%" : $"+{diff}";
                            var newVal = resultMod.percent ? $"{resultMod.value}%" : $"{resultMod.value}";
                            
                            // Rich text formatting for preview difference
                            sb.AppendLine($"<color=#FFB300><b>{mod.stat}: {mod.value} ➔ <color=#4CAF50>{newVal} ({diffVal})</color></b></color>");
                            continue;
                        }
                    }
                }
                
                // Fallback to normal display if no result weapon or no difference
                sb.AppendLine($"<color=#FFB300><b>{mod.stat}: {val}</b></color>");
            }

            // 2. In ra Dòng Phụ (Màu Trắng/Xám)
            foreach (var mod in weapon.modifiers)
            {
                if (mod.isMainStat) continue;

                var sign = mod.value >= 0 ? "+" : "";
                var val = mod.percent ? $"{sign}{(mod.percentValue * 100).ToString("0.##")}%" : $"{sign}{mod.value}";
                
                if (resultWeapon != null && resultWeapon.modifiers != null)
                {
                    var resultMod = resultWeapon.modifiers.FirstOrDefault(m => m.stat == mod.stat && !m.isMainStat);
                    if (resultMod != null)
                    {
                        // Sub stats value are generally compared via percentValue if they are percent
                        float diff = resultMod.percentValue > 0 ? (resultMod.percentValue - mod.percentValue) * 100 : (resultMod.value - mod.value);
                        if (diff > 0)
                        {
                            var diffVal = mod.percent ? $"+{diff.ToString("0.##")}%" : $"+{diff}";
                            var newVal = mod.percent ? $"{(resultMod.percentValue * 100).ToString("0.##")}%" : $"{resultMod.value}";
                            
                            sb.AppendLine($"  <color=#DDDDDD>• {mod.stat}: {val} ➔ <color=#4CAF50>{newVal} ({diffVal})</color></color>");
                            continue;
                        }
                    }
                }
                
                sb.AppendLine($"  <color=#DDDDDD>• {mod.stat}: {val}</color>");
            }
        }

        statsText.text = sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Refresh material requirements display
    /// </summary>
    private void RefreshMaterialRequirements()
    {
        if (currentRecipe == null) return;

        // Clear existing material slots
        foreach (var slot in spawnedMaterialSlots)
            Destroy(slot);
        spawnedMaterialSlots.Clear();

        var forgingSystem = ForgingSystem.Instance;

        // Display required materials
        if (materialSlotPrefab != null)
        {
            foreach (var req in currentRecipe.requiredMaterials)
            {
                GameObject slot = Instantiate(materialSlotPrefab, materialsListParent);
                spawnedMaterialSlots.Add(slot);

                var materialUI = slot.GetComponent<MaterialSlotUI>();
                if (materialUI != null && req.material != null)
                {
                    int playerHas = forgingSystem.GetMaterialQuantity(req.material.materialID);
                    var material = req.material;
                    materialUI.SetMaterial(material, req.quantity, playerHas);
                }
            }
        }

        // Display required weapons as slots
        if (materialSlotPrefab != null)
        {
            foreach (var reqW in currentRecipe.requiredWeapons)
            {
                if (reqW.weapon == null) continue;
                
                GameObject slot = Instantiate(materialSlotPrefab, materialsListParent);
                spawnedMaterialSlots.Add(slot);

                var materialUI = slot.GetComponent<MaterialSlotUI>();
                if (materialUI != null)
                {
                    // Đếm số lượng vũ khí này trong kho đồ
                    int playerHas = Inventory.Instance.ownedItems.Where(i => i.itemID == reqW.weapon.itemID).Sum(i => i.stack);
                    
                    // Lấy Icon từ vũ khí
                    Sprite wIcon = null;
                    if (!string.IsNullOrEmpty(reqW.weapon.iconPath))
                        wIcon = Resources.Load<Sprite>(reqW.weapon.iconPath);

                    materialUI.SetInfo(wIcon, reqW.weapon.itemName, reqW.quantity, playerHas);
                }
            }
        }

        // Update gold
        if (goldRequiredText != null)
        {
            goldRequiredText.text = $"Gold: {currentRecipe.goldCost}";
        }

        if (weaponsNeededText != null)
        {
            weaponsNeededText.gameObject.SetActive(false); // Đã dùng Slot hiển thị vũ khí, ẩn dòng text cũ đi
        }

        // Check forge button interactability
        UpdateForgeButtonState();
    }

    /// <summary>
    /// Update whether forge button can be clicked
    /// </summary>
    private void UpdateForgeButtonState()
    {
        if (forgeButton == null || currentRecipe == null || selectedWeapon == null) return;

        var forgingSystem = ForgingSystem.Instance;
        var playerStat = PlayerStat.Instance;

        if (forgingSystem == null || playerStat == null || Inventory.Instance == null)
        {
            forgeButton.interactable = false;
            return;
        }

        bool canForge = true;

        // Check Mastery
        float currentMastery = Inventory.Instance.GetClassMastery(currentRecipe.resultItem.weaponClassName);
        if (currentMastery < currentRecipe.requiredMastery)
        {
            canForge = false;
        }

        // Check materials
        if (!forgingSystem.HasMaterials(currentRecipe.requiredMaterials))
            canForge = false;

        // Check gold
        if (!playerStat.CanSpendGold(currentRecipe.goldCost))
            canForge = false;

        forgeButton.interactable = canForge;
    }

    /// <summary>
    /// Handle forge button click
    /// </summary>
    private void OnForgeButtonClicked()
    {
        if (selectedWeapon == null || currentRecipe == null) return;

        // Attempt forge
        var forgeManager = ForgeManager.Instance;
        var materials = currentRecipe.requiredMaterials
            .Select(req => req.material)
            .ToList();

        ItemData forgedWeapon = forgeManager.AttemptForge(currentRecipe, materials);
        if (forgedWeapon != null)
        {
            // Success!
            Debug.Log($"Successfully forged: {forgedWeapon.itemName}");
            RefreshWeaponList();
            ShowWeaponDetail(forgedWeapon);
        }
        else
        {
            Debug.LogError("Forging failed!");
        }
    }

    public void CloseForgeUI()
    {
        if (forgePanel != null && forgePanel.activeSelf)
        {
            forgePanel.SetActive(false);
            if (detailPanel != null) detailPanel.SetActive(false);
            if (recipeBookPanel != null) recipeBookPanel.SetActive(false);
            if (advancedWeaponsPanel != null) advancedWeaponsPanel.SetActive(false);
            
            if (CursorManager.Instance != null) CursorManager.Instance.SetUIOpen(false);
        }
    }

    private List<GameObject> spawnedRecipeSlots = new List<GameObject>();

    /// <summary>
    /// Open the Recipe Book popup and populate it with all available recipes grouped by Weapon Class
    /// </summary>
    public void OpenRecipeBook()
    {
        if (recipeBookPanel == null) return;
        recipeBookPanel.SetActive(true);

        // Clear old slots
        foreach (var slot in spawnedRecipeSlots) Destroy(slot);
        spawnedRecipeSlots.Clear();

        if (recipeBookContentParent == null || recipeBookSlotPrefab == null) return;

        var forgingSystem = ForgingSystem.Instance;
        var forgeManager = ForgeManager.Instance;
        if (forgingSystem == null || forgeManager == null) return;

        // Group recipes by weapon class of the result item
        var recipesByClass = forgingSystem.recipes
            .Select(r => new { Recipe = r, ResultItem = r.resultItem != null ? new ItemData(r.resultItem.itemID) : forgeManager.GetWeaponTemplate(r.resultItemID) })
            .Where(x => x.ResultItem != null)
            .GroupBy(x => x.ResultItem.weaponClassName)
            .OrderBy(g => g.Key); // Sort by class (e.g. Bow, Greatsword...)

        foreach (var group in recipesByClass)
        {
            // Note: You can spawn a "Header" prefab here to show group.Key (the class name) if you want to visually separate them.

            foreach (var item in group)
            {
                GameObject slot = Instantiate(recipeBookSlotPrefab, recipeBookContentParent);
                spawnedRecipeSlots.Add(slot);

                var slotUI = slot.GetComponent<RecipeBookSlotUI>();
                if (slotUI != null)
                {
                    slotUI.SetRecipe(item.Recipe, item.ResultItem);
                }
            }
        }
    }

    /// <summary>
    /// Open the Advanced Weapons popup and show weapons of the same class
    /// </summary>
    public void ShowAdvancedWeapons(ItemData baseWeapon)
    {
        if (advancedWeaponsPanel == null || advancedWeaponsContentParent == null || weaponSlotPrefab == null)
        {
            Debug.LogWarning("[ForgeUI] Advanced Weapons Panel or Prefab is not assigned.");
            return;
        }

        advancedWeaponsPanel.SetActive(true);
        advancedWeaponsPanel.transform.SetAsLastSibling();

        // Clear old slots
        foreach (var slot in spawnedAdvancedWeaponSlots) Destroy(slot);
        spawnedAdvancedWeaponSlots.Clear();

        var forgeManager = ForgeManager.Instance;
        if (forgeManager == null) return;

        // Filter advanced weapons by class
        var classWeapons = forgeManager.LoadAdvancedWeaponsForClass(baseWeapon.weaponClassName).ToList();

        foreach (var weapon in classWeapons)
        {
            GameObject slot = Instantiate(weaponSlotPrefab, advancedWeaponsContentParent);
            spawnedAdvancedWeaponSlots.Add(slot);

            var slotUI = slot.GetComponent<WeaponSlotUI>();
            if (slotUI != null)
            {
                slotUI.SetWeapon(weapon, () => {
                    PreviewAdvancedWeapon(weapon);
                });
                slotUI.SetInteractable(true); // Always allow previewing the weapon
            }
        }
    }

    /// <summary>
    /// Preview an advanced weapon in the result slot and compare stats
    /// </summary>
    private void PreviewAdvancedWeapon(ItemData advancedWeapon)
    {
        if (selectedWeapon == null) return;

        // Find the recipe that produces this advanced weapon
        currentRecipe = ForgingSystem.Instance?.recipes.FirstOrDefault(r => (r.resultItem != null ? r.resultItem.itemID : r.resultItemID) == advancedWeapon.itemID);

        // Refresh materials display
        RefreshMaterialRequirements();

        // Update the result preview group to show this advanced weapon
        if (resultPreviewGroup != null) resultPreviewGroup.SetActive(true);
        if (resultWeaponIcon != null) resultWeaponIcon.sprite = string.IsNullOrEmpty(advancedWeapon.iconPath) ? null : Resources.Load<Sprite>(advancedWeapon.iconPath);
        if (resultWeaponNameText != null) resultWeaponNameText.text = advancedWeapon.itemName;

        // Compare stats between base weapon and selected advanced weapon
        UpdateStatsDisplay(selectedWeapon, advancedWeapon);
        
        // Close the popup after selecting
        if (advancedWeaponsPanel != null) advancedWeaponsPanel.SetActive(false);
    }
}

/// <summary>
/// UI for individual recipe in the Recipe Book popup
/// </summary>
public class RecipeBookSlotUI : MonoBehaviour
{
    public Image resultWeaponIcon;
    public TMP_Text resultWeaponNameText;
    public TMP_Text weaponClassText;
    public TMP_Text requirementsText; // To list what is needed (e.g. Base Weapon + Materials)

    public void SetRecipe(ForgingRecipe recipe, ItemData resultWeapon)
    {
        if (resultWeaponIcon != null)
            resultWeaponIcon.sprite = string.IsNullOrEmpty(resultWeapon.iconPath) ? null : Resources.Load<Sprite>(resultWeapon.iconPath);
        
        if (resultWeaponNameText != null)
            resultWeaponNameText.text = resultWeapon.itemName;
            
        if (weaponClassText != null)
            weaponClassText.text = $"Class: {resultWeapon.weaponClassName}";

        if (requirementsText != null)
        {
            var sb = new System.Text.StringBuilder();
            
            // Show base weapon required
            if (recipe.requiredWeapons != null && recipe.requiredWeapons.Count > 0)
            {
                var reqW = recipe.requiredWeapons[0];
                if (reqW != null && reqW.weapon != null) sb.Append($"Base: {reqW.weapon.itemName}\n");
            }

            // Show materials
            if (recipe.requiredMaterials != null && recipe.requiredMaterials.Count > 0)
            {
                sb.Append("Mats: ");
                var forgingSystem = ForgingSystem.Instance;
                foreach (var req in recipe.requiredMaterials)
                {
                    var mat = req.material;
                    if (mat != null) sb.Append($"{req.quantity}x {mat.materialName}, ");
                }
            }
            
            requirementsText.text = sb.ToString().TrimEnd(',', ' ');
        }
    }
}

/// <summary>
/// Simple progress bar component for mastery display
/// </summary>
public class ProgressBar : MonoBehaviour
{
    public Image fillImage;
    public TMP_Text valueText;

    public void SetValue(float current, float max)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = current / max;
        }
    }
}
