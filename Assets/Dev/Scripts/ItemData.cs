using System;
using UnityEngine;

[Serializable]
public class ItemData
{
    public string itemID;
    public string itemName;
    public string description;
    public int price;
    public string iconPath; // Resources path (e.g. "Sprites/Icons/firestaff")
    public int stack = 1;

    public ItemData() { }

    public ItemData(string id, string name, string desc, int p, string icon, int s = 1)
    {
        itemID = id;
        itemName = name;
        description = desc;
        price = p;
        iconPath = icon;
        stack = s;
    }
}
