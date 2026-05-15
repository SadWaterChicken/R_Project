using System;
using System.Collections.Generic;

[Serializable]
public class ShopDataJson
{
    public string shopName;
    public List<ItemData> items = new List<ItemData>();
}
