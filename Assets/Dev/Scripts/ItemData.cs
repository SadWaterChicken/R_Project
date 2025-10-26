using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemData
{
    public string itemID;
    public string itemName;
    public string description;
    public int price;
    public string iconPath;
    public int stack = 1;

    // Flexible stat modifiers
    [Serializable]
    public class StatMod
    {
        public string stat;   // e.g. "Attack", "Defense", "MaxHealth", "CritChance", "MoveSpeed"
        public float value;   // 10 or -5
        public bool percent;  // true => percent (10 = +10%)
    }
    public List<StatMod> modifiers = new List<StatMod>();

    // Equip flags
    public bool equippable = true;
    public bool equipped = false;

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
