using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ShopEntry
{
    public ItemSO item;
    public int priceOverride; // 0 => use basePrice
    public int stock = 10;
    public bool unlimited = false;
}

[CreateAssetMenu(menuName = "RPG/ShopData")]
public class ShopData : ScriptableObject
{
    public string shopName;
    public List<ShopEntry> entries = new List<ShopEntry>();
    public string requiredQuestIdToEnableSocketing; // optional for Physic shop
}
