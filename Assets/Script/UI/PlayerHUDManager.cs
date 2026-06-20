using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý giao diện HUD của Player hiển thị trong quá trình chơi game
/// (Thanh máu HP, Mana, Sanity, hiển thị Minimap)
/// </summary>
public class PlayerHUDManager : MonoBehaviour
{
    public static PlayerHUDManager Instance { get; private set; }

    [Header("HP Bar (Thanh máu) - Slider hoặc Image Fill")]
    public Slider hpSlider;
    public Image hpFillImage;
    public TMP_Text hpText;

    [Header("Mana Bar (Thanh mana) - Slider hoặc Image Fill")]
    public Slider manaSlider;
    public Image manaFillImage;
    public TMP_Text manaText;

    [Header("Sanity Bar (Thanh sanity) - Slider hoặc Image Fill")]
    public Slider sanitySlider;
    public Image sanityFillImage;
    public TMP_Text sanityText;

    [Header("Minimap References (Tùy chọn)")]
    public RawImage minimapRawImage;

    [Header("Update Interval Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    private float updateTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateHUD();
    }

    private void Update()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            UpdateHUD();
            updateTimer = updateInterval;
        }
    }

    /// <summary>
    /// Cập nhật các thông tin hiển thị trên HUD từ PlayerStat
    /// </summary>
    public void UpdateHUD()
    {
        if (PlayerStat.Instance == null) return;

        // ── 1. Cập nhật thanh HP ──────────────────────────────────────────
        float currentHP = PlayerStat.Instance.currentHealth;
        float maxHP     = PlayerStat.Instance.maxHealth;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value    = Mathf.Clamp(currentHP, 0f, maxHP);
        }
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = maxHP > 0f ? (currentHP / maxHP) : 0f;
        }
        if (hpText != null)
        {
            hpText.text = $"{Mathf.Max(0, Mathf.RoundToInt(currentHP))} / {Mathf.RoundToInt(maxHP)}";
        }

        // ── 2. Cập nhật thanh Mana ────────────────────────────────────────
        float currentMana = PlayerStat.Instance.currentMana;
        float maxMana     = PlayerStat.Instance.maxMana;

        if (manaSlider != null)
        {
            manaSlider.maxValue = maxMana;
            manaSlider.value    = Mathf.Clamp(currentMana, 0f, maxMana);
        }
        if (manaFillImage != null)
        {
            manaFillImage.fillAmount = maxMana > 0f ? (currentMana / maxMana) : 0f;
        }
        if (manaText != null)
        {
            manaText.text = $"{Mathf.Max(0, Mathf.RoundToInt(currentMana))} / {Mathf.RoundToInt(maxMana)}";
        }

        // ── 3. Cập nhật thanh Sanity ──────────────────────────────────────
        float currentSanity = PlayerStat.Instance.currentSanity;
        float maxSanity     = PlayerStat.Instance.maxSanity;

        if (sanitySlider != null)
        {
            sanitySlider.maxValue = maxSanity;
            sanitySlider.value    = Mathf.Clamp(currentSanity, 0f, maxSanity);
        }
        if (sanityFillImage != null)
        {
            sanityFillImage.fillAmount = maxSanity > 0f ? (currentSanity / maxSanity) : 0f;
        }
        if (sanityText != null)
        {
            sanityText.text = $"{Mathf.Max(0, Mathf.RoundToInt(currentSanity))} / {Mathf.RoundToInt(maxSanity)}";
        }
    }
}
