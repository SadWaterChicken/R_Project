using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý giao diện HUD của Player hiển thị trong quá trình chơi game
/// (Thanh máu HP, Level người chơi, hiển thị Minimap)
/// </summary>
public class PlayerHUDManager : MonoBehaviour
{
    public static PlayerHUDManager Instance { get; private set; }

    [Header("Player HP Bar (Tùy chọn Slider hoặc Image Fill)")]
    public Slider hpSlider;
    public Image hpFillImage;
    public TMP_Text hpText;

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

        // 1. Cập nhật thanh máu (HP)
        float currentHP = PlayerStat.Instance.currentHealth;
        float maxHP = PlayerStat.Instance.maxHealth;

        // Nếu dùng Slider
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = Mathf.Clamp(currentHP, 0f, maxHP);
        }

        // Nếu dùng Image Fill (thuộc tính Fill Amount của Image)
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = maxHP > 0f ? (currentHP / maxHP) : 0f;
        }

        if (hpText != null)
        {
            hpText.text = $"{Mathf.Max(0, Mathf.RoundToInt(currentHP))} / {Mathf.RoundToInt(maxHP)}";
        }
    }
}
