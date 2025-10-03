using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Controller - Hiển thị Health, Mana, Sanity bars và stats
/// </summary>
public class PlayerUIController : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerData playerData;

    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Gradient healthColorGradient;

    [Header("Mana UI")]
    [SerializeField] private Slider manaSlider;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Image manaFillImage;
    [SerializeField] private Color manaColor = Color.blue;

    [Header("Sanity UI")]
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private TextMeshProUGUI sanityText;
    [SerializeField] private Image sanityFillImage;
    [SerializeField] private Gradient sanityColorGradient;

    [Header("Stats UI")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemsText;

    [Header("Sanity Heal Button")]
    [SerializeField] private Button sanityHealButton;
    [SerializeField] private TextMeshProUGUI sanityHealCostText;

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool animateBarChanges = true;

    private float targetHealthValue;
    private float targetManaValue;
    private float targetSanityValue;

    private void Start()
    {
        if (playerData == null)
        {
            playerData = FindFirstObjectByType<PlayerData>();
        }

        if (playerData != null)
        {
            // Subscribe to events
            playerData.OnHealthChanged += OnHealthChanged;
            playerData.OnManaChanged += OnManaChanged;
            playerData.OnSanityChanged += OnSanityChanged;

            // Initialize UI
            InitializeUI();
        }

        // Setup sanity heal button
        if (sanityHealButton != null)
        {
            sanityHealButton.onClick.AddListener(OnSanityHealButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (playerData != null)
        {
            playerData.OnHealthChanged -= OnHealthChanged;
            playerData.OnManaChanged -= OnManaChanged;
            playerData.OnSanityChanged -= OnSanityChanged;
        }
    }

    private void Update()
    {
        if (animateBarChanges)
        {
            AnimateBars();
        }

        UpdateSanityHealButton();
    }

    private void InitializeUI()
    {
        UpdateHealthUI(playerData.CurrentHealth, playerData.MaxHealth);
        UpdateManaUI(playerData.CurrentMana, playerData.MaxMana);
        UpdateSanityUI(playerData.CurrentSanity, playerData.MaxSanity);
        UpdateCurrencyUI();
    }

    #region Event Handlers
    private void OnHealthChanged(int current, int max)
    {
        UpdateHealthUI(current, max);
    }

    private void OnManaChanged(int current, int max)
    {
        UpdateManaUI(current, max);
    }

    private void OnSanityChanged(int current, int max)
    {
        UpdateSanityUI(current, max);
    }
    #endregion

    #region Update UI
    private void UpdateHealthUI(int current, int max)
    {
        float normalizedValue = (float)current / max;
        targetHealthValue = normalizedValue;

        if (!animateBarChanges && healthSlider != null)
        {
            healthSlider.value = normalizedValue;
        }

        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }

        if (healthFillImage != null && healthColorGradient != null)
        {
            healthFillImage.color = healthColorGradient.Evaluate(normalizedValue);
        }
    }

    private void UpdateManaUI(int current, int max)
    {
        float normalizedValue = (float)current / max;
        targetManaValue = normalizedValue;

        if (!animateBarChanges && manaSlider != null)
        {
            manaSlider.value = normalizedValue;
        }

        if (manaText != null)
        {
            manaText.text = $"{current}/{max}";
        }

        if (manaFillImage != null)
        {
            manaFillImage.color = manaColor;
        }
    }

    private void UpdateSanityUI(int current, int max)
    {
        float normalizedValue = (float)current / max;
        targetSanityValue = normalizedValue;

        if (!animateBarChanges && sanitySlider != null)
        {
            sanitySlider.value = normalizedValue;
        }

        if (sanityText != null)
        {
            sanityText.text = $"{current}/{max}";
        }

        if (sanityFillImage != null && sanityColorGradient != null)
        {
            sanityFillImage.color = sanityColorGradient.Evaluate(normalizedValue);
        }
    }

    private void UpdateCurrencyUI()
    {
        if (playerData == null) return;

        if (goldText != null)
        {
            goldText.text = $"Gold: {playerData.Gold}";
        }

        if (gemsText != null)
        {
            gemsText.text = $"Gems: {playerData.Gems}";
        }
    }
    #endregion

    #region Animation
    private void AnimateBars()
    {
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealthValue, Time.deltaTime * smoothSpeed);
        }

        if (manaSlider != null)
        {
            manaSlider.value = Mathf.Lerp(manaSlider.value, targetManaValue, Time.deltaTime * smoothSpeed);
        }

        if (sanitySlider != null)
        {
            sanitySlider.value = Mathf.Lerp(sanitySlider.value, targetSanityValue, Time.deltaTime * smoothSpeed);
        }
    }
    #endregion

    #region Sanity Heal Button
    private void UpdateSanityHealButton()
    {
        if (sanityHealButton == null || playerData == null) return;

        // Enable button nếu có đủ sanity và chưa full HP
        bool canHeal = playerData.CurrentSanity >= 20 && playerData.CurrentHealth < playerData.MaxHealth;
        sanityHealButton.interactable = canHeal;

        if (sanityHealCostText != null)
        {
            sanityHealCostText.text = $"-20 Sanity";
        }
    }

    private void OnSanityHealButtonClicked()
    {
        if (playerData != null)
        {
            bool success = playerData.HealUsingSanity();
            
            if (success)
            {
                // Visual feedback
                Debug.Log("Healed using Sanity!");
                // TODO: Add heal effect animation
            }
        }
    }
    #endregion

    /// <summary>
    /// Call this để cập nhật currency UI khi có thay đổi
    /// </summary>
    public void RefreshCurrencyUI()
    {
        UpdateCurrencyUI();
    }
}
