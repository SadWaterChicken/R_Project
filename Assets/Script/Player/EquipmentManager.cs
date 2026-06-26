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
    private WeaponController lastAttackingWeapon;

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

        // 2. Tạo Parent Weapon Object (Wrapper) để tránh làm bẩn Prefab gốc
        GameObject parentObj = new GameObject($"[Weapon] {item.itemName}");
        parentObj.transform.SetParent(targetSocket);
        parentObj.transform.localPosition = Vector3.zero;
        parentObj.transform.localRotation = Quaternion.identity;
        
        WeaponController weaponController = parentObj.AddComponent<WeaponController>();

        // 3. Spawn thẳng Weapon Prefab (Giờ đây nó TỰ CHỨA Component Animator nếu cần)
        GameObject modelObj = Instantiate(wData.weaponPrefab, parentObj.transform, false);
        modelObj.name = "3D_Model";

        // 4. Tìm Animator trên Prefab gốc để gán cho vũ khí
        Animator weaponAnim = modelObj.GetComponent<Animator>();
        if (weaponAnim == null)
        {
            weaponAnim = modelObj.GetComponentInChildren<Animator>();
        }
        weaponController.baseAnimator = weaponAnim;

        // Xử lý Event Tấn Công (Nếu vũ khí có Animator, gắn Event Handler vào để nhận HitFrame)
        if (weaponAnim != null)
        {
            AnimationEventHandler eventHandler = weaponAnim.gameObject.GetComponent<AnimationEventHandler>();
            if (eventHandler == null)
            {
                eventHandler = weaponAnim.gameObject.AddComponent<AnimationEventHandler>();
            }
            if (eventHandler.OnEventTriggered == null)
            {
                eventHandler.OnEventTriggered = new UnityEngine.Events.UnityEvent<string>();
            }
            eventHandler.OnEventTriggered.AddListener(weaponController.HandleAnimationEvent);
        }

        // 5. Initialize WeaponController (Tự nạp dữ liệu vào Component)
        weaponController.Initialize(item);

        // 6. Lưu trữ Reference (Không ghi đè Animator của người chơi)
        if (item.equipSlot == EquipSlot.MainHand)
        {
            currentMainHandWeapon = weaponController;
            Debug.Log($"[EquipmentManager] Đã trang bị {item.itemName}. Weapon Animator và Player Animator sẽ chạy độc lập song song.");
        }
        else
        {
            currentOffHandWeapon = weaponController;
        }

        // Cập nhật dáng đứng (WeaponStance) cho Player Animator gốc (Hệ thống cũ/dự phòng)
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
            if (playerAnimator != null) 
            {
                playerAnimator.SetInteger("WeaponStance", 0); // Về dáng tay không
            }
        }
        else if (slot == EquipSlot.OffHand && currentOffHandWeapon != null)
        {
            Destroy(currentOffHandWeapon.gameObject);
            currentOffHandWeapon = null;
        }
    }

    public void TriggerMainHandAttack(string triggerName = "Attack")
    {
        if (currentMainHandWeapon != null)
        {
            lastAttackingWeapon = currentMainHandWeapon;
            currentMainHandWeapon.Attack(triggerName);
        }
    }

    public void TriggerOffHandAttack(string triggerName = "Attack")
    {
        if (currentOffHandWeapon != null)
        {
            lastAttackingWeapon = currentOffHandWeapon;
            currentOffHandWeapon.Attack(triggerName);
        }
    }

    public void TriggerMainHandSkill()
    {
        if (currentMainHandWeapon != null)
        {
            lastAttackingWeapon = currentMainHandWeapon;
            currentMainHandWeapon.UseSkill();
        }
    }

    public void TriggerOffHandSkill()
    {
        if (currentOffHandWeapon != null)
        {
            lastAttackingWeapon = currentOffHandWeapon;
            currentOffHandWeapon.UseSkill();
        }
    }

    // Forward events từ Player Animator xuống cho tất cả vũ khí đang cầm
    private void ForwardEventToWeapons(string eventName)
    {
        // 1. Phân tách rõ ràng nếu Animation truyền event cụ thể
        if (eventName == "MainHand_Hit" && currentMainHandWeapon != null)
        {
            currentMainHandWeapon.HandleAnimationEvent("HitFrame");
            return;
        }
        if (eventName == "OffHand_Hit" && currentOffHandWeapon != null)
        {
            currentOffHandWeapon.HandleAnimationEvent("HitFrame");
            return;
        }

        // 2.(Fallback) cho event "HitFrame" chung
        if (lastAttackingWeapon != null)
        {
            // Chỉ gửi sát thương cho vũ khí vừa vung
            lastAttackingWeapon.HandleAnimationEvent(eventName);
        }
        else
        {
            // Nếu không biết vũ khí nào (VD: Múa cả 2 tay cùng lúc nhưng chưa gán lastAttackingWeapon) thì cho cả 2 nổ damage
            if (currentMainHandWeapon != null) currentMainHandWeapon.HandleAnimationEvent(eventName);
            if (currentOffHandWeapon != null) currentOffHandWeapon.HandleAnimationEvent(eventName);
        }
    }

    public bool HasOffHandWeapon()
    {
        return currentOffHandWeapon != null;
    }

    public CombatStyle GetOffHandCombatStyle()
    {
        if (currentOffHandWeapon != null && currentOffHandWeapon.weaponData != null)
        {
            return currentOffHandWeapon.weaponData.combatStyle;
        }
        return CombatStyle.Melee; // Default fallback
    }
}
