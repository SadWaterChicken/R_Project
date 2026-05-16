# ✅ FIX: Gold không cập nhật + Sell button không hoạt động đúng

## 🔴 Vấn Đề Đã Giải Quyết

### Vấn Đề 1: Gold hiển thị 0 dù chỉnh 50000
**Nguyên nhân**: ShopUI chỉ lấy gold lúc mở shop, không update khi bạn chỉnh trực tiếp trên Inspector
**Giải pháp**: Cập nhật để ShopUI luôn lấy gold mới nhất từ PlayerStat

### Vấn Đề 2: Sell button sáng lên dù không có item
**Nguyên nhân**: Sell button không được quản lý bởi ShopUI, không có logic kiểm tra inventory
**Giải pháp**: Thêm Sell button management vào ShopUI + logic kiểm tra inventory

---

## 🔧 Các Thay Đổi Được Thực Hiện

### 1. ShopUI.cs
```diff
+ public Button sellButton;  ← Thêm reference

+ int currentGold = playerStat != null ? playerStat.GetGold() : 0;
+ UpdateGoldText(currentGold);  ← Luôn lấy gold mới từ PlayerStat

+ // Setup Sell button
+ if (sellButton != null)
+ {
+     sellButton.onClick.RemoveAllListeners();
+     sellButton.onClick.AddListener(() => manager.SellItem(item));
+     
+     bool hasItem = Inventory.Instance != null && 
+                   Inventory.Instance.ownedItems.Exists(x => x.itemID == item.itemID);
+     sellButton.interactable = hasItem;  ← Chỉ sáng khi có item
+ }
```

### 2. ShopManager.cs
```diff
+ public void SellItem(ItemData item)
+ {
+     // Check inventory
+     var inventoryItem = Inventory.Instance.ownedItems.Find(x => x.itemID == item.itemID);
+     
+     // Remove item
+     Inventory.Instance.RemoveItem(inventoryItem, 1);
+     
+     // Add gold
+     int sellPrice = Mathf.RoundToInt(item.price * 0.5f);  // 50% of buy price
+     playerStat.AddGold(sellPrice);
+     
+     // Update UI
+     shopUI.UpdateGoldText(playerStat.GetGold());
+ }
```

---

## 🎯 Cách Sử Dụng

### Setup trong Inspector

#### Bước 1: Gán Sell Button
1. Select **ShopUI** (UI Canvas)
2. Tìm **ShopUI (Script)** component
3. Kéo thả nút **Sell** vào field **Sell Button**

```
ShopUI (Script):
├─ Buy Button: (nút Buy)
├─ Sell Button: (kéo nút Sell vào đây) ← NEW!
└─ Close Button: (nút Close)
```

### Test

#### Test 1: Gold Update
1. Mở game
2. Chỉnh Gold = 50000 trên Player Inspector (hoặc trong code)
3. Mở Shop
4. **Kết quả**: Gold hiển thị 50000 ✓ (không phải 0)

#### Test 2: Sell Button
1. Có item trong Inventory
2. Mở Shop
3. Click vào item
4. **Kết quả**: Sell button sáng lên ✓
5. Click Sell
6. **Kết quả**: Item bị remove, gold được cộng thêm ✓

#### Test 3: Sell Button Disabled
1. Không có item trong Inventory
2. Mở Shop
3. Click vào item
4. **Kết quả**: Sell button mờ đi (disabled) ✓

---

## 📊 Tóm Tắt Sửa

| Vấn Đề | Nguyên Nhân | Giải Pháp |
|--------|-----------|----------|
| **Gold hiển thị 0** | ShopUI lấy gold lúc mở, không update | Lấy gold luôn mới từ PlayerStat |
| **Sell button sáng sai** | Không check inventory | Thêm logic `hasItem` check |
| **Sell không hoạt động** | Không có SellItem method | Thêm SellItem vào ShopManager |

---

## ✅ Build Status

✅ **Build successful** (0 errors)

---

## 🎮 Next Steps

1. **Assign Sell Button** trong Inspector (ShopUI)
2. **Play game** và test
3. **Enjoy!** Shop hoạt động đúng ✓

---

## 💡 Ghi Chú

- **Sell price = 50% của buy price** (có thể sửa trong ShopManager.SellItem)
- **Sell button chỉ sáng nếu có item** ✓
- **Gold update khi buy/sell** ✓

