using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour
{
    public List<ItemInstance> items = new List<ItemInstance>();
    public int maxSlots = 40;

    public bool HasSpaceFor(ItemSO so, int amount = 1)
    {
        if (so.stackable)
        {
            var s = items.FirstOrDefault(i => i.template == so && i.qty < so.maxStack);
            if (s != null) return true;
        }
        return items.Count < maxSlots;
    }

    public bool AddItem(ItemSO so, int amount = 1)
    {
        if (so.stackable)
        {
            var stack = items.Find(i => i.template == so && i.qty < so.maxStack);
            if (stack != null)
            {
                int addable = Mathf.Min(amount, so.maxStack - stack.qty);
                stack.qty += addable;
                amount -= addable;
                if (amount <= 0) return true;
            }
        }
        while (amount > 0)
        {
            if (items.Count >= maxSlots) return false;
            int add = so.stackable ? Mathf.Min(amount, so.maxStack) : 1;
            var inst = new ItemInstance(so, add);
            inst.template = so;
            items.Add(inst);
            amount -= add;
        }
        return true;
    }

    public bool RemoveItem(ItemSO so, int amount = 1)
    {
        for (int i = items.Count - 1; i >= 0 && amount > 0; i--)
        {
            if (items[i].template == so)
            {
                int remove = Mathf.Min(items[i].qty, amount);
                items[i].qty -= remove;
                amount -= remove;
                if (items[i].qty <= 0) items.RemoveAt(i);
            }
        }
        return amount == 0;
    }
}
