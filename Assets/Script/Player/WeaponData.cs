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

public enum CombatActionType
{
    Execute,
    Charge,
    Continuous
}



[CreateAssetMenu(fileName = "New Weapon", menuName = "Item Data/Weapon Data")]
public class WeaponData : BaseItemData
{
    [Header("Visuals & Combat")]
    public GripType gripType = GripType.OneHanded;
    public CombatStyle combatStyle = CombatStyle.Melee;

    [Header("PlayerCombatStateMachine Routing Data")]
    public CombatActionType actionType = CombatActionType.Execute;


    [Header("Combat Stats")]
    [Tooltip("Dùng cho CombatStyle = Magic/Ranged")]
    public GameObject projectilePrefab;

    [Tooltip("Controller chứa State Machine hoạt ảnh riêng cho vũ khí này")]
    public RuntimeAnimatorController weaponAnimatorController;

    [Header("Component-Based System")]
    [Tooltip("Bạn không cần gõ tên Component nữa! Hãy kéo thả các script (như MeleeDamageComponent) TRỰC TIẾP vào file Prefab của vũ khí trên Editor!")]
    public string instructionInfo = "Gắn Component thẳng vào Prefab";

    private void OnEnable()
    {
        // Ép kiểu EquipmentType thành Weapon để đồng bộ logic chung
        equipmentType = EquipmentType.Weapon; // Tạm dùng PrimaryWeapon đến khi cấu trúc lại Inventory
    }
}
