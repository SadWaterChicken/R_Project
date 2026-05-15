# Hệ Thống Tương Tác (Interaction System) - Hóp Lý

## 📋 Tổng Quan

Tất cả tương tác trong game (Shop, Inventory, Door/Portal) hiện được điều khiển thống nhất thông qua **PlayerController** với cơ chế **IInteractable Interface**.

---

## 🎮 Key Bindings (Phím Tương Tác)

| Phím | Chức Năng | Xử Lý Bởi | Ghi Chú |
|------|----------|----------|--------|
| **F** | Interact (Shop/Door/Portal) | PlayerController + IInteractable | InputSystem |
| **I** | Toggle Inventory | InventoryInput | KeyCode.I |
| **Q** | Rotate Camera Left | PlayerController | -90° (nếu không interact) |
| **E** | Rotate Camera Right | PlayerController | +90° (nếu không interact) |
| **WASD** | Move | PlayerController | InputSystem |
| **Space** | Sprint/Dash | PlayerController | InputSystem |

---

## 🔄 Luồng Tương Tác

### 1. **Shop Interaction**
```
Player đứng gần Shop
     ↓
PlayerController.DetectNearbyInteractables() 
     ↓ (tìm IInteractable trong range)
Tìm thấy ShopTrigger (IInteractable)
     ↓
Player nhấn F
     ↓
inputActions.Player.Interact.triggered = true
     ↓
nearbyInteractable.Interact() 
     ↓ (gọi ShopTrigger.Interact())
ShopUI mở, hiển thị sản phẩm
```

### 2. **Door/Portal Interaction** (tương tự Shop)
```
Player đứng gần Door/Portal (IInteractable)
     ↓
Player nhấn F
     ↓
Door/Portal.Interact() được gọi
     ↓
Cửa mở / Portal kích hoạt
```

### 3. **Inventory Toggle**
```
Player nhấn I (bất kỳ lúc nào)
     ↓
InventoryInput.Update() → Input.GetKeyDown(KeyCode.I)
     ↓
InventoryUI.Toggle()
     ↓
Inventory mở/đóng (không phụ thuộc vào PlayerController)
```

### 4. **Camera Rotation**
```
Player nhấn Q hoặc E (khi không ở gần Shop/Door/Portal)
     ↓
PlayerController kiểm tra: nearbyInteractable == null?
     ↓ (nếu không có interactable gần)
Quay camera -90° (Q) hoặc +90° (E)
     ↓ (nếu có interactable gần)
KHÔNG quay camera (ưu tiên tương tác)
```

---

## 📂 Files Chính

### PlayerController.cs
**Vị trí**: `Assets\Script\Player\PlayerController.cs`
**Chức năng**:
- Quản lý input chính (Movement, Camera, Interaction)
- Detect nearby IInteractable objects
- Gọi `Interact()` khi F được nhấn
- Quay camera khi Q/E được nhấn (nếu không có interactable)

**Code quan trọng**:
```csharp
// Detect interactables
DetectNearbyInteractables();

// Interact input
if (inputActions.Player.Interact.triggered && nearbyInteractable != null)
{
    nearbyInteractable.Interact();
}

// Camera rotation (chỉ khi không interact)
if (nearbyInteractable == null && Input.GetKeyDown(KeyCode.Q)) 
    targetYRotation -= 90f;
if (nearbyInteractable == null && Input.GetKeyDown(KeyCode.E)) 
    targetYRotation += 90f;
```

---

### ShopTrigger.cs
**Vị trí**: `Assets\Script\ShopSystem\ShopTrigger.cs`
**Chức năng**:
- Implement `IInteractable` interface
- Load shop JSON từ StreamingAssets
- Mở ShopUI khi `Interact()` được gọi

**Thay đổi**:
- ✅ Loại bỏ `playerInRange` check trong Update()
- ✅ Loại bỏ `Input.GetKeyDown(KeyCode.E)` logic
- ✅ Thêm `public void Interact()` method
- ✅ Giữ lại trigger collider detection (OnTriggerEnter/Exit để show hint)

**Code**:
```csharp
public class ShopTrigger : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        // Mở shop
        shopManager.OpenShop(cachedShop);
    }
}
```

---

### InventoryInput.cs
**Vị trí**: `Assets\Script\Inventory\InventoryInput.cs`
**Chức năng**:
- Singleton để xử lý Inventory toggle input
- Dùng KeyCode.I (không dùng InputSystem vì không có Inventory action)

**Code**:
```csharp
if (Input.GetKeyDown(KeyCode.I))
{
    ui.Toggle();
}
```

---

### IInteractable.cs
**Vị trí**: `Assets\Script\New_Dungeon\IInteractable.cs`
**Interface**:
```csharp
public interface IInteractable
{
    void Interact();
}
```

**Các class implement**:
- ShopTrigger
- Door (nếu có)
- Portal (nếu có)
- Bất kỳ object có thể tương tác nào

---

## 🛑 Deprecated/Loại Bỏ

### ❌ Interactor.cs
- **Trạng thái**: Không dùng nữa
- **Lý do**: PlayerController hiện xử lý tất cả interaction detection
- **Khuyến cáo**: Có thể xóa hoặc giữ lại nếu cần fallback

### ❌ ShopTrigger.Update() với KeyCode.E
- **Trạng thái**: Đã loại bỏ
- **Lý do**: Xung đột với camera rotation
- **Thay thế**: Dùng IInteractable.Interact() gọi từ PlayerController

---

## ✅ Lợi Ích của Hệ Thống Mới

1. **Không Xung Đột**: E key không còn dùng cho Shop, chỉ dùng cho camera
2. **Hợp Nhất**: Tất cả interaction dùng chung F key + IInteractable
3. **Dễ Mở Rộng**: Thêm object interact mới chỉ cần implement IInteractable
4. **Input Tập Trung**: PlayerController là điểm kiểm soát chính
5. **Hint Logic**: Trigger collider vẫn show hint "Press F to Interact"

---

## 🎯 Cách Thêm Interaction Mới

### Bước 1: Tạo script implement IInteractable
```csharp
public class MyInteractable : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        // Làm gì đó
        Debug.Log("Interacted!");
    }
}
```

### Bước 2: Add Collider 2D (trigger)
- Thêm Collider2D component
- Tick "Is Trigger"
- Set Layer/Tag phù hợp (PlayerController dùng Physics.OverlapSphere)

### Bước 3: Done!
- PlayerController sẽ tự detect object này
- Player nhấn F sẽ gọi Interact()

---

## 📊 Diagram

```
┌─────────────────────────────────────────────┐
│         PlayerController                     │
│  - Movement                                  │
│  - Camera Rotation                           │
│  - Input Handling                            │
└──────────────┬────────────────────────────────┘
               │
        ┌──────▼──────────────────────────────┐
        │  DetectNearbyInteractables()        │
        │  (Physics.OverlapSphere)            │
        └──────┬───────────────────────────────┘
               │
        ┌──────▼────────────────────────────────┐
        │  Tìm IInteractable objects:            │
        │  ├─ ShopTrigger                       │
        │  ├─ Door                              │
        │  ├─ Portal                            │
        │  └─ ...                               │
        └──────┬────────────────────────────────┘
               │
        ┌──────▼────────────────────────────────┐
        │  F key pressed?                        │
        │  nearbyInteractable != null?           │
        └──────┬────────────────────────────────┘
               │
        ┌──────▼────────────────────────────────┐
        │  nearbyInteractable.Interact()         │
        │  (Polymorphic call)                    │
        └──────┬────────────────────────────────┘
               │
       ┌───────┴────────────────┬────────────────┐
       │                        │                │
  ┌────▼───────┐     ┌─────────▼───┐   ┌────────▼──┐
  │ShopTrigger │     │Door/Portal   │   │ ... etc   │
  │.Interact() │     │.Interact()   │   │           │
  └────────────┘     └──────────────┘   └───────────┘
```

---

## 🐛 Troubleshooting

### Shop không mở khi nhấn F?
- ✅ Check ShopTrigger có Collider2D + Is Trigger = true?
- ✅ Check ShopManager assigned?
- ✅ Check shopJsonFile path đúng?
- ✅ Check player có tag "Player"?

### Camera vẫn quay khi gần Shop?
- ✅ Bây giờ không nên xảy ra (logic kiểm tra nearbyInteractable)
- ✅ Nếu vẫn xảy ra, kiểm tra DetectNearbyInteractables()

### F key không hoạt động?
- ✅ Check InputSystem.Player.Interact action mapped to F?
- ✅ Check InputSystem_Actions enabled?
- ✅ Check có IInteractable object gần?

---

## 📝 Summary

**Trước**: Shop dùng E (xung đột), Inventory dùng I, Door dùng F
**Sau**: Tất cả interaction dùng F (qua IInteractable), Inventory vẫn I, E/Q cho camera (nếu không interact)

**Kết quả**: Hệ thống đơn giản, hợp lý, dễ mở rộng ✅
