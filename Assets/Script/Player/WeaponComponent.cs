using UnityEngine;

public abstract class WeaponComponent : MonoBehaviour
{
    protected WeaponController controller;

    public virtual void Initialize(WeaponController weaponController)
    {
        controller = weaponController;
    }

    // Lắng nghe sự kiện Animation
    public virtual void OnAnimationEvent(string eventName) { }
}
