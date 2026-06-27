using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Data")]
    public WeaponData weaponData;
    public ItemData currentItemData; // Chứa modifier thực tế (sau khi rèn)

    [Header("Hierarchy")]
    public Animator baseAnimator;
    
    private List<WeaponComponent> activeComponents = new List<WeaponComponent>();

    public void Initialize(ItemData item)
    {
        currentItemData = item;
        weaponData = item.BaseData as WeaponData;
        
        if (weaponData == null)
        {
            Debug.LogError($"[WeaponController] Cannot cast BaseItemData to WeaponData for item: {item.itemID}");
            return;
        }

        // Lấy tất cả Component trên game object này VÀ CÁC OBJECT CON (như 3D Model Prefab)
        WeaponComponent[] comps = GetComponentsInChildren<WeaponComponent>();
        activeComponents.AddRange(comps);


        foreach (var comp in activeComponents)
        {
            comp.Initialize(this);
        }
    }

    public void Attack()
    {
        if (baseAnimator != null && baseAnimator.runtimeAnimatorController != null)
        {
            baseAnimator.SetTrigger("Attack");
        }
    }

    public void UseSkill()
    {
        if (weaponData != null && weaponData.hasSkill)
        {
            if (baseAnimator != null && baseAnimator.runtimeAnimatorController != null)
            {
                baseAnimator.SetTrigger(weaponData.weaponSkill.animationTrigger);
            }
        }
        else
        {
            Debug.LogWarning($"[WeaponController] Vũ khí {weaponData?.itemName} chưa được cấu hình Skill!");
        }
    }

    // Được gọi bởi AnimationEventHandler
    public void HandleAnimationEvent(string eventName)
    {
        if (eventName == "FireSkill")
        {
            if (weaponData != null && weaponData.hasSkill && weaponData.weaponSkill.skillPrefab != null)
            {
                // Đẻ Prefab kỹ năng ra tại vị trí của vũ khí
                GameObject skillObj = Instantiate(weaponData.weaponSkill.skillPrefab, transform.position, transform.rotation);
                
                // Trả về cho code của bạn của bạn tự xử lý tiếp
                Debug.Log($"[WeaponController] Đã xả Skill: {weaponData.weaponSkill.skillName}");
            }
        }

        foreach (var comp in activeComponents)
        {
            comp.OnAnimationEvent(eventName);
        }
    }
}
