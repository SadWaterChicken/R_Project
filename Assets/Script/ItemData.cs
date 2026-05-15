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

    // Flexible stat mods shown in detailStats
    [Serializable]
    public class StatMod
    {
        public string stat;
        public float value;
        public bool percent;
    }
    public List<StatMod> modifiers = new List<StatMod>();

    // Equip flags (optional)
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

    // Deep copy so stats are preserved when adding/buying
    public ItemData Clone(int? stackOverride = null)
    {
        var copy = new ItemData(itemID, itemName, description, price, iconPath, stackOverride ?? stack)
        {
            equippable = equippable,
            equipped = equipped,
            modifiers = new List<StatMod>()
        };
        if (modifiers != null)
        {
            foreach (var m in modifiers)
                copy.modifiers.Add(new StatMod { stat = m.stat, value = m.value, percent = m.percent });
        }
        return copy;
    }
}
