using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý thanh máu World Space Canvas hiển thị nổi trên đầu kẻ địch
/// </summary>
public class EnemyHealthBarWorld : MonoBehaviour
{
    [Header("Enemy Reference")]
    [Tooltip("Nếu để trống, Script sẽ tự tìm trong các component cha")]
    public EnemyStat enemyStat;

    [Header("UI Reference")]
    [Tooltip("Thanh Slider hiển thị HP của quái")]
    public Slider healthSlider;

    [Header("Settings")]
    [Tooltip("Nếu true, thanh máu sẽ tự ẩn đi khi quái đầy máu")]
    public bool hideWhenFullHealth = true;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // Tự động tìm EnemyStat nếu chưa gán
        if (enemyStat == null)
        {
            enemyStat = GetComponentInParent<EnemyStat>();
        }

        // Tự động tìm Slider nếu chưa gán
        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
        }

        if (enemyStat == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Không tìm thấy EnemyStat trên đối tượng hoặc cha của nó!");
        }

        if (healthSlider != null && enemyStat != null)
        {
            healthSlider.maxValue = enemyStat.maxHealth;
            healthSlider.value = enemyStat.currentHealth;
        }
    }

    private void Update()
    {
        if (enemyStat == null || healthSlider == null) return;

        float curHP = enemyStat.currentHealth;
        float maxHP = enemyStat.maxHealth;

        // Cập nhật giá trị thanh máu
        healthSlider.maxValue = maxHP;
        healthSlider.value = curHP;

        // Tự động ẩn khi đầy máu (nếu cài đặt)
        if (hideWhenFullHealth)
        {
            bool isFull = Mathf.Approximately(curHP, maxHP);
            healthSlider.gameObject.SetActive(!isFull);
        }

        // Xoay Canvas về phía Camera (Billboard effect)
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            // Xoay thanh máu cùng góc nhìn với Camera để tránh biến dạng trong không gian 3D/2.5D
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
}
