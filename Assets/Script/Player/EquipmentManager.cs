using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Sockets")]
    public Transform mainHandSocket;
    public Transform offHandSocket;

    private GameObject currentMainHandWeapon;
    private GameObject currentOffHandWeapon;

    private Animator playerAnimator;

    private void Awake()
    {
        playerAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged += HandleEquipChange;
            
            // Khởi tạo vũ khí ban đầu (nếu có lúc load game)
            foreach (var item in Inventory.Instance.ownedItems)
            {
                if (item.equipped && item.equipmentType == EquipmentType.PrimaryWeapon)
                {
                    HandleEquipChange(item, true);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged -= HandleEquipChange;
        }
    }

    private void HandleEquipChange(ItemData item, bool isEquipped)
    {
        if (item.equipmentType != EquipmentType.PrimaryWeapon) return;

        if (isEquipped)
        {
            EquipWeapon(item);
        }
        else
        {
            UnequipWeapon(item.equipSlot);
        }
    }

    private void EquipWeapon(ItemData item)
    {
        if (item.BaseData == null || item.BaseData.weaponPrefab == null)
        {
            Debug.LogWarning($"[EquipmentManager] {item.itemName} không có Weapon Prefab!");
            return;
        }

        Transform targetSocket = (item.equipSlot == EquipSlot.MainHand) ? mainHandSocket : offHandSocket;
        if (targetSocket == null)
        {
            Debug.LogError($"[EquipmentManager] Chưa gán {item.equipSlot} Socket trên nhân vật!");
            return;
        }

        // Xóa vũ khí cũ nếu có
        UnequipWeapon(item.equipSlot);

        // Sinh ra vũ khí mới
        GameObject newWeaponObj = Instantiate(item.BaseData.weaponPrefab, targetSocket);
        newWeaponObj.transform.localPosition = Vector3.zero;
        newWeaponObj.transform.localRotation = Quaternion.identity;

        // Lưu trữ reference
        if (item.equipSlot == EquipSlot.MainHand)
        {
            currentMainHandWeapon = newWeaponObj;
        }
        else
        {
            currentOffHandWeapon = newWeaponObj;
        }

        // Gửi sát thương vào vũ khí
        MeleeWeapon meleeScript = newWeaponObj.GetComponent<MeleeWeapon>();
        if (meleeScript != null)
        {
            float totalPhysicalDamage = 0f;
            foreach(var mod in item.modifiers)
            {
                if (mod.stat == "physicalDamage") totalPhysicalDamage += mod.value;
            }
            meleeScript.Initialize(totalPhysicalDamage, item.equipSlot);
        }

        // Cập nhật dáng đứng (WeaponStance) cho Player Animator
        if (playerAnimator != null && item.equipSlot == EquipSlot.MainHand)
        {
            playerAnimator.SetInteger("WeaponStance", item.BaseData.customStanceID);
        }
    }

    private void UnequipWeapon(EquipSlot slot)
    {
        if (slot == EquipSlot.MainHand && currentMainHandWeapon != null)
        {
            Destroy(currentMainHandWeapon);
            currentMainHandWeapon = null;
            if (playerAnimator != null) playerAnimator.SetInteger("WeaponStance", 0); // Về dáng tay không
        }
        else if (slot == EquipSlot.OffHand && currentOffHandWeapon != null)
        {
            Destroy(currentOffHandWeapon);
            currentOffHandWeapon = null;
        }
    }

    public void TriggerMainHandAttack()
    {
        if (currentMainHandWeapon != null)
        {
            Animator weaponAnim = currentMainHandWeapon.GetComponent<Animator>();
            if (weaponAnim != null) weaponAnim.SetTrigger("Attack");
        }
    }

    public void TriggerOffHandAttack()
    {
        if (currentOffHandWeapon != null)
        {
            Animator weaponAnim = currentOffHandWeapon.GetComponent<Animator>();
            if (weaponAnim != null) weaponAnim.SetTrigger("Attack");
        }
    }
}
