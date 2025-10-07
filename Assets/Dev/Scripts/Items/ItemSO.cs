using UnityEngine;

public enum ItemType { Consumable, Weapon, Armor, Accessory, Material, Gem, Staff }

[CreateAssetMenu(menuName = "RPG/Item")]
public class ItemSO : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite icon;
    public ItemType type;
    public int basePrice = 10;
    public bool stackable = true;
    public int maxStack = 99;
    public int socketCount = 0;
    public int basePower = 0;
    [TextArea] public string description;
}
