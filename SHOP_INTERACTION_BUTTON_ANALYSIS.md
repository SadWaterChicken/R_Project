# Shop Interaction Button Analysis

## Summary
**Nút tương tác với Shop: `E` (nhấn E để mở Shop)**

---

## Chi Tiết Về Các Nút Input Trong Game

### 1. **Shop Interaction** 
- **Nút**: `KeyCode.E`
- **File**: `Assets\Script\ShopSystem\ShopTrigger.cs` (dòng 103)
- **Code**:
```csharp
if (!(playerInRange && Input.GetKeyDown(KeyCode.E))) return;
```
- **Hoạt động**: 
  - Khi player đứng trong vùng trigger của Shop (playerInRange = true)
  - Nhấn phím `E` để mở Shop
  - Hiển thị prompt "Press E" bằng `interactHint` GameObject

---

### 2. **Camera Rotation** 
- **Nút**: `KeyCode.Q` (quay trái) và `KeyCode.E` (quay phải)
- **File**: `Assets\Script\Player\PlayerController.cs` (dòng 117-118)
- **Code**:
```csharp
if (Input.GetKeyDown(KeyCode.Q)) targetYRotation -= 90f;
if (Input.GetKeyDown(KeyCode.E)) targetYRotation += 90f;
```
- **Hoạt động**: 
  - `Q` quay camera 90° sang trái (-90°)
  - `E` quay camera 90° sang phải (+90°)
  - Có smooth rotation với `rotationSpeed = 5f`

---

### 3. **Tình Huống Trùng Lặp** ⚠️

**YES - CÓ XUNG ĐỘT GIỮA SHOP VÀ CAMERA:**
- Nút `E` được dùng cho **2 mục đích khác nhau**:
  1. **Shop Interaction**: Mở Shop (chỉ khi playerInRange = true)
  2. **Camera Rotation**: Quay camera sang phải

**Giải Quyết Hiện Tại**:
- `ShopTrigger.cs` check `playerInRange` trước khi xử lý
- Nếu player **KHÔNG** ở trong vùng Shop trigger:
  - `E` sẽ quay camera (PlayerController xử lý)
- Nếu player **CÓ** ở trong vùng Shop trigger:
  - `E` sẽ mở Shop (ShopTrigger xử lý)

---

### 4. **Các Nút Input Khác**

| Nút | Chức Năng | File | Ghi Chú |
|-----|----------|------|---------|
| **WASD / Stick Analog** | Di chuyển | PlayerController | InputSystem |
| **Space** | Dash/Sprint | PlayerController | InputSystem |
| **Q** | Quay Camera Trái | PlayerController | -90° rotation |
| **E** | Quay Camera Phải hoặc Mở Shop | PlayerController + ShopTrigger | **XUNG ĐỘT** |
| **F** | Tương tác Cửa/Portal | Interactor | IInteractable |
| **Click Chuột Trái** | Tấn Công | PlayerCombat (old) | Không có trong Dev version |

---

### 5. **Các Interaction System**

#### a) **ShopTrigger** (Shop System)
- **Kích hoạt**: Collider trigger 2D
- **Điều kiện**: Player ở trong vùng + nhấn E
- **Xử lý**:
  1. Tải Shop JSON từ StreamingAssets
  2. Gọi `shopManager.OpenShop(shopData)`
  3. Hiển thị ShopUI

#### b) **Interactor** (Cửa/Portal)
- **Kích hoạt**: Physics.OverlapSphere trong InteractRange
- **Nút**: `KeyCode.F`
- **Xử lý**: Gọi `interactable.Interact()`

#### c) **PlayerController** (Interactable Detection)
- **Kích hoạt**: Physics.OverlapSphere trong interactionRange
- **Nút**: InputActions.Player.Interact (cấu hình trong Input System Asset)
- **Xử lý**: Gọi `nearbyInteractable.Interact()`

---

## Khuyến Cáo ⚠️

### Nên Làm:
1. **Thay đổi nút Shop từ `E` sang `F`** để tránh xung đột camera rotation
   - `E` là nút quay camera chính
   - `F` đã được dùng cho Interactor nhưng có thể unify

2. **Hoặc**: Kiểm tra `playerInRange` trong PlayerController trước khi quay camera:
```csharp
// PlayerController.cs
private ShopTrigger nearbyShop;

// Ở Update():
if (Input.GetKeyDown(KeyCode.E) && nearbyShop == null)
{
    targetYRotation += 90f; // Quay camera
}
```

3. **Hoặc**: Dùng lệnh Interact thay vì E riêng cho Shop:
   - Hợp nhất cả Shop, Cửa, Portal vào 1 hệ thống Interactor

---

## Code Locations

### ShopTrigger.cs - E Key Handler
📍 `Assets\Script\ShopSystem\ShopTrigger.cs` (dòng 101-130)

### PlayerController.cs - Camera Rotation
📍 `Assets\Script\Player\PlayerController.cs` (dòng 117-118)

### Interactor.cs - Cửa/Portal (F Key)
📍 `Assets\Script\Player\Interactor.cs` (dòng 23)

---

## Kết Luận

**Nhấn `E` để tương tác với Shop** nhưng có xung đột với camera rotation.

Để tránh nhầm lẫn, nên:
1. Thay Shop sang nút khác (F, E là backup camera)
2. Hoặc thêm check playerInRange trong camera rotation logic
3. Hoặc merge tất cả interact logic vào 1 system
