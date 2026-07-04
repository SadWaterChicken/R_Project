using UnityEngine;

[CreateAssetMenu(fileName = "New Active Skill", menuName = "Skill System/Active Skill Data")]
public class ActiveSkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillID;
    public string skillName;
    [TextArea(2, 4)] public string description;
    public Sprite skillIcon;
    public string weaponClassRequirement = ""; // vd: "Battlesword", để trống nếu vũ khí nào cũng xài được

    [Header("Combat Stats")]
    public float manaCost = 20f;
    public float cooldown = 5f;
    public float baseDamageMultiplier = 1.2f; // Ví dụ: 1.2 = 120% ATK

    [Header("Animation & Prefab")]
    public string animationTrigger = "hit1";
    public GameObject skillPrefab;
}
