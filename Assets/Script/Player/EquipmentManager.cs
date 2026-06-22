using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Sockets")]
    public Transform mainHandSocket;
    public Transform offHandSocket;

    private WeaponController currentMainHandWeapon;
    private WeaponController currentOffHandWeapon;

    private Animator playerAnimator;
    private AnimationEventHandler playerAnimEventHandler;

    private void Awake()
    {
        playerAnimator = GetComponentInChildren<Animator>();
        if (playerAnimator != null)
        {
            playerAnimEventHandler = playerAnimator.GetComponent<AnimationEventHandler>();
            if (playerAnimEventHandler == null)
            {
                playerAnimEventHandler = playerAnimator.gameObject.AddComponent<AnimationEventHandler>();
            }
            // Khởi tạo UnityEvent nếu chưa có
            if (playerAnimEventHandler.OnEventTriggered == null)
            {
                playerAnimEventHandler.OnEventTriggered = new UnityEngine.Events.UnityEvent<string>();
            }
            playerAnimEventHandler.OnEventTriggered.AddListener(ForwardEventToWeapons);
        }
    }

    private void Start()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged += HandleEquipChange;
            
            // Khởi tạo vũ khí ban đầu (nếu có lúc load game)
            foreach (var item in Inventory.Instance.ownedItems)
            {
                if (item.equipped && item.equipmentType == EquipmentType.Weapon)
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
        if (item.equipmentType != EquipmentType.Weapon) return;

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

        WeaponData wData = item.BaseData as WeaponData;
        if (wData == null)
        {
            // Tạm thời nếu vẫn dùng BaseItemData cũ thì giữ logic cũ, nhưng in cảnh báo
            Debug.LogWarning($"[EquipmentManager] Đang nâng cấp cấu trúc. Cần tạo lại {item.itemName} dưới dạng WeaponData.");
            return;
        }

        Transform targetSocket = (item.equipSlot == EquipSlot.MainHand) ? mainHandSocket : offHandSocket;
        if (targetSocket == null)
        {
            Debug.LogError($"[EquipmentManager] Chưa gán {item.equipSlot} Socket trên nhân vật!");
            return;
        }

        // 1. Xóa vũ khí cũ nếu có
        UnequipWeapon(item.equipSlot);

        // 2. Tạo Parent Weapon Object (Root)
        GameObject parentObj = new GameObject($"[Weapon] {item.itemName}");
        parentObj.transform.SetParent(targetSocket);
        parentObj.transform.localPosition = Vector3.zero;
        parentObj.transform.localRotation = Quaternion.identity;
        
        WeaponController weaponController = parentObj.AddComponent<WeaponController>();

        // 3. Tạo Base Game Object (Core Animator)
        GameObject baseObj = new GameObject("BaseAnimator");
        baseObj.transform.SetParent(parentObj.transform);
        baseObj.transform.localPosition = Vector3.zero;
        baseObj.transform.localRotation = Quaternion.identity;

        Animator baseAnim = baseObj.AddComponent<Animator>();
        if (wData.weaponAnimatorController != null)
        {
            baseAnim.runtimeAnimatorController = wData.weaponAnimatorController;
        }
        
        AnimationEventHandler eventHandler = baseObj.AddComponent<AnimationEventHandler>();
        eventHandler.OnEventTriggered = new UnityEngine.Events.UnityEvent<string>();
        eventHandler.OnEventTriggered.AddListener(weaponController.HandleAnimationEvent);

        weaponController.baseAnimator = baseAnim;

        // 4. Tạo 3D Weapon Model (Thực thể hiển thị) trước, vì các Component giờ nằm trên nó!
        // Tham số 'false' giúp giữ nguyên tọa độ Local (vị trí đã căn sẵn) của Prefab
        GameObject modelObj = Instantiate(wData.weaponPrefab, baseObj.transform, false);
        modelObj.name = "3D_Model";

        // 3.5 Quét và cài đặt các Component Logic đã được gắn sẵn trên Prefab (chuẩn Video)
        WeaponComponent[] attachedComponents = modelObj.GetComponentsInChildren<WeaponComponent>();
        foreach (WeaponComponent comp in attachedComponents)
        {
            // Các Component đang nằm trên modelObj hoặc con của nó, WeaponController (nằm trên Parent) sẽ quản lý chúng
            // Vì WeaponController tìm Component ở trên GameObject của NÓ, ta chuyển Component lên Parent nếu muốn,
            // hoặc đơn giản là sửa WeaponController để tìm Component trên toàn bộ cây Parent.
            // Sửa lại: Ta KHÔNG dời Component đi, ta để nguyên trên Prefab. 
            // Vòng lặp này chỉ để log ra thông tin (việc rà soát thực sự nằm ở WeaponController.Initialize).
            Debug.Log($"[EquipmentManager] Found component {comp.GetType().Name} on {wData.itemName} prefab.");
        }

        // 5. Initialize WeaponController (Tự nạp dữ liệu vào Component)
        weaponController.Initialize(item);

        // 6. Lưu trữ Reference
        if (item.equipSlot == EquipSlot.MainHand)
        {
            currentMainHandWeapon = weaponController;
        }
        else
        {
            currentOffHandWeapon = weaponController;
        }

        // Cập nhật dáng đứng (WeaponStance) cho Player Animator gốc (Ví dụ: cách cầm vũ khí)
        if (playerAnimator != null && item.equipSlot == EquipSlot.MainHand)
        {
            if (System.Array.Exists(playerAnimator.parameters, p => p.name == "WeaponStance"))
            {
                playerAnimator.SetInteger("WeaponStance", item.BaseData.customStanceID);
            }
        }
    }

    private void UnequipWeapon(EquipSlot slot)
    {
        if (slot == EquipSlot.MainHand && currentMainHandWeapon != null)
        {
            Destroy(currentMainHandWeapon.gameObject);
            currentMainHandWeapon = null;
            if (playerAnimator != null) playerAnimator.SetInteger("WeaponStance", 0); // Về dáng tay không
        }
        else if (slot == EquipSlot.OffHand && currentOffHandWeapon != null)
        {
            Destroy(currentOffHandWeapon.gameObject);
            currentOffHandWeapon = null;
        }
    }

    public void TriggerMainHandAttack()
    {
        if (currentMainHandWeapon != null)
        {
            currentMainHandWeapon.Attack();
        }
    }

    public void TriggerOffHandAttack()
    {
        if (currentOffHandWeapon != null)
        {
            currentOffHandWeapon.Attack();
        }
    }

    // Forward events từ Player Animator xuống cho tất cả vũ khí đang cầm
    private void ForwardEventToWeapons(string eventName)
    {
        if (currentMainHandWeapon != null)
        {
            currentMainHandWeapon.HandleAnimationEvent(eventName);
        }
        if (currentOffHandWeapon != null)
        {
            currentOffHandWeapon.HandleAnimationEvent(eventName);
        }
    }

    public bool HasOffHandWeapon()
    {
        return currentOffHandWeapon != null;
    }
}
