using System;
using System.Collections.Generic;

[Serializable]
public class ItemInstance
{
    public string instanceId;
    public string itemId; // store id to remap to ItemSO at load
    public int qty; // stackable items only
    public int level; // upgrade level, 0 = base item
    public List<string> socketItemIds = new List<string>(); // ds id của mấy cái gem socket vào item này

    // helper - runtime only reference (not serialized)
    [NonSerialized] public ItemSO template;

    public ItemInstance() { instanceId = Guid.NewGuid().ToString(); }
    public ItemInstance(ItemSO so, int quantity = 1)
    {
        instanceId = Guid.NewGuid().ToString();
        template = so;
        itemId = so.id;
        qty = quantity;
        level = 0;
    }
}
