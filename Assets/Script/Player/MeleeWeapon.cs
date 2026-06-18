using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeleeWeapon : MonoBehaviour
{
    private float physicalDamage;
    private EquipSlot weaponSlot;
    private Collider weaponCollider;

    private void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        weaponCollider.isTrigger = true;
        weaponCollider.enabled = false; // Chỉ bật khi có Animation vung vũ khí
    }

    public void Initialize(float damage, EquipSlot slot)
    {
        this.physicalDamage = damage;
        this.weaponSlot = slot;
    }

    // Hàm này sẽ được Animation Event gọi trên vũ khí khi bắt đầu chém
    public void EnableHitbox()
    {
        if (weaponCollider != null) weaponCollider.enabled = true;
    }

    // Hàm này sẽ được Animation Event gọi trên vũ khí khi chém xong
    public void DisableHitbox()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Tìm component EnemyStat để trừ máu
            // (Giả sử bạn có script EnemyStat, nếu tên khác thì bạn đổi lại)
            var enemyStat = other.GetComponentInParent<EnemyStat>();
            if (enemyStat != null)
            {
                // Truyền sát thương
                enemyStat.TakePhysicalDamage(physicalDamage);
                
                // Hiệu ứng máu me / âm thanh có thể thêm vào đây
                Debug.Log($"[{weaponSlot}] Vung trúng đích! Gây {physicalDamage} sát thương vật lý.");
            }
        }
    }
}
