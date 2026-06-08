using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "RPG/Item Asset")]
public class ItemAsset : ScriptableObject
{
    [Header("Item Configuration")]
    public ItemData itemData;
}
