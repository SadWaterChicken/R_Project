using UnityEngine;

public enum GripType
{
    OneHanded,
    TwoHanded
}

public enum CombatStyle
{
    Melee,
    Ranged,
    Magic,
    Defend
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Item Data/Weapon Data")]
public class WeaponData : BaseItemData
{
    [Header("Visuals & Combat")]
    public GripType gripType = GripType.OneHanded;
    public CombatStyle combatStyle = CombatStyle.Melee;

    [Header("Combat Stats")]
    [Tooltip("Dùng cho CombatStyle = Magic/Ranged")]
    public GameObject projectilePrefab;

    [Tooltip("Controller chứa State Machine hoạt ảnh riêng cho vũ khí này")]
    public RuntimeAnimatorController weaponAnimatorController;

    [Header("Component-Based System")]
    [Tooltip("Danh sách tên các Script/Component sẽ tự động được gán vào vũ khí khi trang bị (VD: MeleeDamageComponent, ProjectileSpawnerComponent)")]
    public System.Collections.Generic.List<string> weaponComponents = new System.Collections.Generic.List<string>();
    
    private void OnEnable()
    {
        // Ép kiểu EquipmentType thành Weapon để đồng bộ logic chung
        equipmentType = EquipmentType.Weapon; // Tạm dùng PrimaryWeapon đến khi cấu trúc lại Inventory
    }
}
