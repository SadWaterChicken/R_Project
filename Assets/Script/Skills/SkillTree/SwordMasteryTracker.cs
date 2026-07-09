using UnityEngine;

/// <summary>
/// Theo dõi Mastery EXP khi người chơi dùng kiếm (Greatsword) để chiến đấu.
///
/// QUY TẮC TÍNH SP:
///   - masteryExp (0–100) được lưu trong Inventory JSON (Greatsword)
///   - Mỗi 10 masteryExp = 1 Skill Point (SP)
///   - Ở masteryExp = 100 (max), tặng thêm 1 SP bonus → Tổng max SP = 11
///
/// Cách dùng:
///   Gọi SwordMasteryTracker.Instance.AddMasteryExp(amount) từ bất kỳ đâu
///   khi giết địch. Tracker tự động sync với Inventory.
/// </summary>
public class SwordMasteryTracker : MonoBehaviour
{
    public static SwordMasteryTracker Instance { get; private set; }

    // ─── CONSTANTS ────────────────────────────────────────────────────────────
    public const float MAX_MASTERY_EXP   = 100f;   // masteryExp max trong Inventory
    public const float EXP_PER_SP        = 10f;    // cứ 10 EXP = 1 SP
    public const int   MAX_NORMAL_SP     = 10;     // SP từ mastery thường (0-90 EXP → 9 SP, 90-100 → 1 SP bonus)
    public const int   BONUS_SP_AT_MAX   = 1;      // bonus thêm khi đạt 100 EXP
    public const int   TOTAL_MAX_SP      = 11;     // tổng max SP có thể nhận

    // expPerLevel giữ lại để UI tương thích cũ
    public float expPerLevel => EXP_PER_SP;

    [Header("Debug / View Only")]
    [SerializeField] private float currentExp = 0f;     // 0–100 (từ Inventory JSON)
    [SerializeField] private int   spGranted  = 0;      // Tổng SP đã cấp (để tránh cấp lại)

    // ─── LIFECYCLE ────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Đọc dữ liệu từ Inventory JSON (đồng bộ với hệ thống save chính)
        LoadFromInventory();
    }

    private void Update()
    {
        float inventoryExp = currentExp; // Mặc định giữ nguyên nếu ko có Inventory

        if (Inventory.Instance != null)
        {
            inventoryExp = Inventory.Instance.GetClassMastery("Greatsword");
        }

        if (inventoryExp > currentExp)
        {
            float oldExp = currentExp;
            currentExp = Mathf.Min(inventoryExp, MAX_MASTERY_EXP);
            RecalculateAndGrantSP(oldExp, currentExp);
        }
    }

    // ─── PUBLIC API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Cộng EXP mastery.
    /// Gọi từ logic combat (kill enemy, hit enemy...).
    /// </summary>
    public void AddMasteryExp(float amount)
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.AddClassMastery("Greatsword", amount);
        }
        else
        {
            // Hỗ trợ test UI trong scene độc lập không có Inventory
            float oldExp = currentExp;
            currentExp = Mathf.Min(currentExp + amount, MAX_MASTERY_EXP);
            PlayerPrefs.SetFloat("SwordMastery_Exp", currentExp);
            PlayerPrefs.Save();
            RecalculateAndGrantSP(oldExp, currentExp);
        }
    }

    public void ResetExp()
    {
        currentExp = 0f;
        spGranted = 0;
        PlayerPrefs.SetFloat("SwordMastery_Exp", 0f);
        PlayerPrefs.Save();
        if (Inventory.Instance != null)
        {
            // Nếu có hàm SetClassMastery thì dùng, không thì tạm thời gọi Add âm
            // Ghi chú: Cần Inventory hỗ trợ set đè.
            // Tạm thời ko can thiệp sâu vào Inventory gốc.
        }
    }

    // Getter tương thích với AutoSkillTreeUI
    public int   GetMasteryLevel() => Mathf.FloorToInt(currentExp / EXP_PER_SP);
    public float GetCurrentExp()   => currentExp;
    public float GetExpProgress01() => currentExp / MAX_MASTERY_EXP; // 0→1

    // ─── SP CALCULATION ───────────────────────────────────────────────────────

    /// <summary>
    /// Tính tổng SP nên có ở mức masteryExp hiện tại.
    /// Formula:
    ///   normalSP = floor(exp / 10) → max 10 SP tại 100 EXP
    ///   bonusSP  = 1 nếu exp >= 100
    ///   total    = min(normalSP + bonusSP, 11)
    /// </summary>
    public static int CalcTotalSPForExp(float exp)
    {
        int normalSP = Mathf.FloorToInt(exp / EXP_PER_SP);          // 0–10
        int bonusSP  = (exp >= MAX_MASTERY_EXP) ? BONUS_SP_AT_MAX : 0;
        return Mathf.Min(normalSP + bonusSP, TOTAL_MAX_SP);
    }

    private void RecalculateAndGrantSP(float oldExp, float newExp)
    {
        int totalSPNow  = CalcTotalSPForExp(newExp);
        int totalSPOld  = CalcTotalSPForExp(oldExp);
        int delta       = totalSPNow - totalSPOld;

        if (delta <= 0) return;

        spGranted += delta;

        if (SwordSkillTreeManager.Instance != null)
            SwordSkillTreeManager.Instance.AddSkillPoint(delta);

        if (PlayerStat.Instance != null)
            PlayerStat.Instance.UpdateMasteryDisplay("Greatsword", newExp);

        Debug.Log($"[SwordMasteryTracker] EXP: {oldExp:F1} → {newExp:F1} | +{delta} SP | Total SP granted: {spGranted}");
    }

    // ─── SYNC WITH INVENTORY ──────────────────────────────────────────────────
    private void LoadFromInventory()
    {
        // Đọc từ Inventory.classMasteries["Greatsword"]
        if (Inventory.Instance != null)
        {
            float saved = Inventory.Instance.GetClassMastery("Greatsword");
            currentExp  = Mathf.Clamp(saved, 0f, MAX_MASTERY_EXP);
        }
        else
        {
            // Fallback PlayerPrefs (legacy)
            currentExp = PlayerPrefs.GetFloat("SwordMastery_Exp", 0f);
        }

        // SP đã cấp = tổng SP tương ứng với EXP hiện tại
        spGranted = CalcTotalSPForExp(currentExp);

        Debug.Log($"[SwordMasteryTracker] Loaded — EXP: {currentExp:F1}/{MAX_MASTERY_EXP} | SP đã cấp: {spGranted}");
    }
}

