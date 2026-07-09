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

    // Old Weapon List variables removed

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

    // Advanced Weapons variables removed

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
    
    [Header("Radial Tree Options")]
    public Transform radialTreeObj;
    public Transform classNodesContainer;
    public Transform weaponTreesContainer;
    public Button radialBackButton;
    public Button detailsBackButton;
    [Tooltip("Thanh tiến trình Mastery trên màn hình Radial Tree")]
    public Slider radialMasteryBar;
    [Tooltip("Dòng chữ Mastery trên màn hình Radial Tree")]
    public TMP_Text radialMasteryText;

    [Header("Mastery Overview Options")]
    public Transform masteryOverviewContainer; // Panel tổng quan ở giữa
    public GameObject masteryOverviewRowPrefab; // Dòng UI chứa Text và Slider cho từng Class

    private void Awake()
    {
        forgePanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (ForgingSystem.Instance != null)
            ForgingSystem.Instance.OnMaterialInventoryChanged += RefreshMaterialRequirements;
            
        CursorManager.OnCloseAllUI += CloseForgeUI;
    }

    private void OnDisable()
    {
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



        forgePanel.SetActive(true);
        if (CursorManager.Instance != null) CursorManager.Instance.SetUIOpen(true);
        
        // Auto-hook Radial Tree if generated
        InitRadialTree();
    }

    private void InitRadialTree()
    {
        if (forgePanel == null) return;
        radialTreeObj = forgePanel.transform.Find("RadialForgeTree");
        if (radialTreeObj == null) return;

        classNodesContainer = radialTreeObj.Find("ClassNodes");
        weaponTreesContainer = radialTreeObj.Find("WeaponTrees");
        var backBtnObj = radialTreeObj.Find("BackButton");
        if (backBtnObj != null)
        {
            radialBackButton = backBtnObj.GetComponent<Button>();
        }

        if (radialBackButton != null)
        {
            radialBackButton.onClick.RemoveAllListeners();
            radialBackButton.onClick.AddListener(OnRadialBackClicked);
        }

        if (detailsBackButton != null)
        {
            detailsBackButton.onClick.RemoveAllListeners();
            detailsBackButton.onClick.AddListener(OnDetailsBackClicked);
        }

        // Aggressively hide "Forgeables" text

        foreach (var txt in forgePanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
        {
            if (txt.text.Contains("Forgeable") || txt.text == "Forgeables")
            {
                txt.gameObject.SetActive(false);
            }
        }

        // AGGRESSIVE: Hide the ENTIRE Panel_Content (which holds mastery, icons, and requirements) until a weapon is clicked!
        Transform panelContent = forgePanel.transform.Find("Panel_Content");
        if (panelContent != null) panelContent.gameObject.SetActive(false);

        // Hook up class nodes
        if (classNodesContainer != null)
        {
            var classNodes = classNodesContainer.GetComponentsInChildren<ForgeClassNode>(true);
            foreach (var node in classNodes)
            {
                if (node.nodeButton != null && !string.IsNullOrEmpty(node.className))
                {
                    node.nodeButton.onClick.RemoveAllListeners();
                    var assetName = node.className;
                    node.nodeButton.onClick.AddListener(() => OnRadialClassClicked(assetName));
                }
            }
        }

        // Hook up weapon nodes
        if (weaponTreesContainer != null)
        {
            var weaponNodes = weaponTreesContainer.GetComponentsInChildren<ForgeWeaponNode>(true);
            foreach (var node in weaponNodes)
            {
                if (node.nodeButton != null && node.weaponData != null && !string.IsNullOrEmpty(node.weaponData.itemID))
                {
                    node.nodeButton.onClick.RemoveAllListeners();
                    var wID = node.weaponData.itemID;
                    node.nodeButton.onClick.AddListener(() => OnRadialWeaponClicked(wID));
                }
            }
        }

        // Reset state
        OnRadialBackClicked();
    }

    private void CenterRadialTree(bool centered)
    {
        if (radialTreeObj != null)
        {
            RectTransform rt = radialTreeObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                // If centered, X position is 0. If not (shifted left), X is -200 or anchor is moved.
                // Since anchor is 0.5, 0.5, we just change anchoredPosition.x
                rt.anchoredPosition = new Vector2(centered ? 0 : -350f, 0);
            }
        }
    }

    private void OnRadialBackClicked()
    {
        if (radialTreeObj == null) return;
        
        if (classNodesContainer != null) classNodesContainer.gameObject.SetActive(true);
        if (weaponTreesContainer != null) weaponTreesContainer.gameObject.SetActive(false);
        if (radialBackButton != null) radialBackButton.gameObject.SetActive(false);
        
        // Hide mastery bar when going back to class selection
        if (radialMasteryBar != null) radialMasteryBar.gameObject.SetActive(false);
        if (radialMasteryText != null) radialMasteryText.gameObject.SetActive(false);

        if (detailPanel != null) detailPanel.SetActive(false);
        
        // Hide Panel_Content completely
        Transform panelContent = radialTreeObj.parent.Find("Panel_Content");
        if (panelContent != null) panelContent.gameObject.SetActive(false);
        
        CenterRadialTree(true);

        // Show and populate Mastery Overview
        if (masteryOverviewContainer != null) 
        {
            masteryOverviewContainer.gameObject.SetActive(true);
            PopulateMasteryOverview();
        }
    }

    private void PopulateMasteryOverview()
    {
        if (masteryOverviewContainer == null || masteryOverviewRowPrefab == null || Inventory.Instance == null || ForgeManager.Instance == null) return;

        // Clear old rows
        foreach (Transform child in masteryOverviewContainer)
        {
            if (child.gameObject != masteryOverviewRowPrefab && child.name != "TitleText")
                Destroy(child.gameObject);
        }

        // Gather all unique classes from the weapon database in Resources
        HashSet<string> classes = new HashSet<string>();
        var baseItems = Resources.LoadAll<BaseItemData>("ItemDatabase");
        foreach (var weapon in baseItems)
        {
            if (weapon != null && !string.IsNullOrEmpty(weapon.weaponClassName))
                classes.Add(weapon.weaponClassName);
        }

        float maxMastery = ForgeManager.Instance.GetMaxMastery();

        foreach (string className in classes)
        {
            GameObject row = Instantiate(masteryOverviewRowPrefab, masteryOverviewContainer);
            row.SetActive(true);
            row.name = "MasteryRow_" + className;

            float currentMastery = Inventory.Instance.GetClassMastery(className);

            // Find Text and Slider in the row
            TMP_Text nameText = row.transform.Find("ClassNameText")?.GetComponent<TMP_Text>();
            Slider slider = row.GetComponentInChildren<Slider>();
            TMP_Text valueText = row.transform.Find("ValueText")?.GetComponent<TMP_Text>();

            if (nameText != null) nameText.text = className;
            if (slider != null)
            {
                slider.maxValue = maxMastery;
                slider.value = currentMastery;
            }
            if (valueText != null) valueText.text = $"{currentMastery:F1}";
        }
    }

    private void OnDetailsBackClicked()
    {
        if (detailPanel != null) detailPanel.SetActive(false);
        
        // Hide Panel_Content completely
        Transform panelContent = radialTreeObj?.parent.Find("Panel_Content");
        if (panelContent != null) panelContent.gameObject.SetActive(false);
        
        // Restore Radial Tree
        if (radialTreeObj != null) radialTreeObj.gameObject.SetActive(true);
    }

    private void OnRadialClassClicked(string className)
    {
        if (classNodesContainer != null) classNodesContainer.gameObject.SetActive(false);
        if (weaponTreesContainer != null) weaponTreesContainer.gameObject.SetActive(true);
        if (radialBackButton != null) radialBackButton.gameObject.SetActive(true);

        // 1. Update the Radial Mastery Bar
        float playerMastery = Inventory.Instance != null ? Inventory.Instance.GetClassMastery(className) : 0f;
        if (radialMasteryBar != null || radialMasteryText != null)
        {
            float maxMastery = ForgeManager.Instance != null ? ForgeManager.Instance.GetMaxMastery() : 100f;
            
            if (radialMasteryBar != null)
            {
                radialMasteryBar.maxValue = maxMastery;
                radialMasteryBar.value = playerMastery;
                radialMasteryBar.gameObject.SetActive(true);
            }
            if (radialMasteryText != null)
            {
                radialMasteryText.text = $"Mastery: {playerMastery:F1}/{maxMastery:F0}";
                radialMasteryText.gameObject.SetActive(true);
            }
        }

        // 2. Show only the tree for this class and evaluate node visibility based on Mastery
        foreach (Transform child in weaponTreesContainer)
        {
            bool isCurrentClass = (child.name == "Tree_" + className);
            child.gameObject.SetActive(isCurrentClass);

            if (isCurrentClass)
            {
                var nodes = child.GetComponentsInChildren<ForgeWeaponNode>(true);
                foreach (var node in nodes)
                {
                    if (node.weaponData == null) continue;

                    // Always ensure node is active
                    node.gameObject.SetActive(true);
                    Transform txtChild = node.transform.Find("Text");

                    // Always show Tier 1 (base weapons) normally
                    if (node.weaponData.itemTier <= 1)
                    {
                        if (node.iconImage != null) node.iconImage.color = Color.white;
                        if (node.nodeButton != null) node.nodeButton.interactable = true;
                        if (txtChild != null) txtChild.gameObject.SetActive(true);
                    }
                    else
                    {
                        // Check if player has enough mastery
                        float reqMastery = 0f;
                        if (node.weaponData.isForgeable && node.weaponData.forgingRecipe != null)
                        {
                            reqMastery = node.weaponData.forgingRecipe.requiredMastery;
                        }

                        // Gray out, disable interaction, and hide text if mastery is not enough!
                        bool hasMastery = playerMastery >= reqMastery;
                        if (node.iconImage != null) node.iconImage.color = hasMastery ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f); // Dark gray
                        if (node.nodeButton != null) node.nodeButton.interactable = hasMastery;
                        if (txtChild != null) txtChild.gameObject.SetActive(hasMastery);
                    }
                }
            }
        }

        // Hide detail panel (Requirements) until a weapon is clicked!
        if (detailPanel != null) detailPanel.SetActive(false);
        
        // Hide Panel_Content completely
        Transform panelContent = radialTreeObj.parent.Find("Panel_Content");
        if (panelContent != null) panelContent.gameObject.SetActive(false);
        
        // Hide Mastery Overview
        if (masteryOverviewContainer != null) masteryOverviewContainer.gameObject.SetActive(false);

        CenterRadialTree(true); // Keep centered
    }

    private void OnRadialWeaponClicked(string itemID)
    {
        var forgeManager = ForgeManager.Instance;
        if (forgeManager == null) return;
        
        ItemData weaponData = forgeManager.GetWeaponTemplate(itemID);
        if (weaponData == null) return;

        ItemData resultWeapon = weaponData;
        
        // Find recipe
        var forgingSystem = ForgingSystem.Instance;
        var recipe = forgingSystem.recipes.FirstOrDefault(r => r.resultItem != null && r.resultItem.itemID == itemID);
        if (recipe == null) recipe = forgingSystem.recipes.FirstOrDefault(r => r.resultItemID == itemID);
        
        currentRecipe = recipe;
        
        // Identify Base Weapon
        ItemData baseWeapon = resultWeapon; // Default to self if no recipe
        if (recipe != null && recipe.requiredWeapons != null && recipe.requiredWeapons.Count > 0)
        {
            var reqW = recipe.requiredWeapons[0];
            if (reqW != null && reqW.weapon != null)
            {
                var baseTemplate = forgeManager.GetWeaponTemplate(reqW.weapon.itemID);
                if (baseTemplate != null) baseWeapon = baseTemplate;
            }
        }
        
        // selectedWeapon MUST be the base weapon being consumed
        selectedWeapon = baseWeapon;
        
        if (detailPanel != null) detailPanel.SetActive(true);
        
        // Show Panel_Content so all details (Mastery, Icon, Requirements) are visible
        Transform panelContent = radialTreeObj.parent.Find("Panel_Content");
        if (panelContent != null) panelContent.gameObject.SetActive(true);
        
        // HIDE the Radial Tree while in details view
        if (radialTreeObj != null) radialTreeObj.gameObject.SetActive(false);
        
        if (resultPreviewGroup != null) resultPreviewGroup.SetActive(true);
        if (resultWeaponIcon != null) 
        {
            if (!string.IsNullOrEmpty(resultWeapon.iconPath))
            {
                var sp = Resources.Load<Sprite>(resultWeapon.iconPath);
                if (sp == null)
                {
                    var tex = Resources.Load<Texture2D>(resultWeapon.iconPath);
                    if (tex != null) sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
                resultWeaponIcon.sprite = sp;
            }
            else
            {
                resultWeaponIcon.sprite = null;
            }
        }
        
        if (resultWeaponNameText != null) resultWeaponNameText.text = resultWeapon.itemName;
        if (weaponDescText != null) weaponDescText.text = resultWeapon.description;

        UpdateMasteryDisplay(resultWeapon, null);
        
        // Compare stats between base weapon and result weapon
        UpdateStatsDisplay(baseWeapon, resultWeapon);

        if (currentRecipe != null)
        {
            if (forgeeLevelText != null) forgeeLevelText.text = $"Forge Level: {weaponData.forgeLevel}/{ForgeManager.Instance.GetMaxForgeLevel()}";
            RefreshMaterialRequirements();
        }
        else
        {
            // Base weapon, no recipe
            foreach (var slot in spawnedMaterialSlots) Destroy(slot);
            spawnedMaterialSlots.Clear();
            if (goldRequiredText != null) goldRequiredText.text = "Base Weapon";
            if (forgeButton != null) forgeButton.interactable = false;
        }
    }

    /// <summary>
    /// Refresh the list of weapons player owns that can be forged
    /// </summary>
    private readonly string[] weaponClasses = new string[] {
        "DualBlades", "Staff", "Bow", "Orb", "Greatsaxe", "Greatsword", "Katana", "Warhammer", "Spear"
    };

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
        // Stats
        UpdateStatsDisplay(weapon, null);

        // Reset result preview box to show class name
        currentRecipe = null;
        if (resultPreviewGroup != null) resultPreviewGroup.SetActive(true);
        if (resultWeaponIcon != null) 
        {
            resultWeaponIcon.sprite = null;
            // Optionally set color if needed, but let's leave it as is so the white placeholder shows.
        }
        if (resultWeaponNameText != null) resultWeaponNameText.text = weapon.weaponClassName;
        if (weaponDescText != null) weaponDescText.text = "Click to browse advanced weapons.";

        // Clear requirements panel since no recipe is selected yet
        foreach (var slot in spawnedMaterialSlots) Destroy(slot);
        spawnedMaterialSlots.Clear();
        if (goldRequiredText != null) goldRequiredText.text = "";
        if (weaponsNeededText != null) weaponsNeededText.gameObject.SetActive(false);
        if (forgeButton != null) forgeButton.interactable = false;
    }

    /// <summary>
    /// Update mastery progress bar and text
    /// </summary>
    private void UpdateMasteryDisplay(ItemData weapon, string overrideClassName = null)
    {
        float currentMastery = 0f;
        string className = !string.IsNullOrEmpty(overrideClassName) ? overrideClassName : (weapon != null ? weapon.weaponClassName : "");
        
        if (Inventory.Instance != null && !string.IsNullOrEmpty(className))
        {
            currentMastery = Inventory.Instance.GetClassMastery(className);
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
                var val = mod.percent ? $"{sign}{mod.percentValue.ToString("0.##")}%" : $"{sign}{mod.value}";
                
                if (resultWeapon != null && resultWeapon.modifiers != null)
                {
                    var resultMod = resultWeapon.modifiers.FirstOrDefault(m => m.stat == mod.stat && !m.isMainStat);
                    if (resultMod != null)
                    {
                        // Sub stats value are generally compared via percentValue if they are percent
                        float diff = resultMod.percentValue > 0 ? (resultMod.percentValue - mod.percentValue) : (resultMod.value - mod.value);
                        if (diff > 0)
                        {
                            var diffVal = mod.percent ? $"+{diff.ToString("0.##")}%" : $"+{diff}";
                            var newVal = mod.percent ? $"{resultMod.percentValue.ToString("0.##")}%" : $"{resultMod.value}";
                            
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

        if (materialsListParent == null) 
        {
            Debug.LogError("[ForgeUI] materialsListParent is missing! Please re-assign it in the Inspector or reload your scene without saving.");
            return;
        }

        // Clear existing material slots (including any placeholders left in the Editor)
        foreach (Transform child in materialsListParent)
        {
            Destroy(child.gameObject);
        }
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

        // (Weapon requirements are intentionally no longer displayed, forging is pure material-based now)

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
