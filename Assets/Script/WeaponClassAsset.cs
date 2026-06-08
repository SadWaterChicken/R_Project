using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Class", menuName = "RPG/Weapon Class")]
public class WeaponClassAsset : ScriptableObject
{
    [Header("Class Definition")]
    [Tooltip("Tên class. Ví dụ: Greatsword, Scythe. Phải viết giống hệt trong JSON.")]
    public string className;
    
    [TextArea]
    public string description;

    [Header("Forge Settings")]
    [Tooltip("Hệ số sức mạnh tăng thêm khi rèn trang bị thuộc Class này.")]
    public float forgeMultiplier = 1.0f;
}
