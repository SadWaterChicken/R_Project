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

    // Được gọi bởi AnimationEventHandler
    public void HandleAnimationEvent(string eventName)
    {
        foreach (var comp in activeComponents)
        {
            comp.OnAnimationEvent(eventName);
        }
    }
}
