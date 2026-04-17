# Checklist Cài đặt Hệ thống Dungeon Mới

## ✅ File đã tạo

- [x] **RoomPrefabManager.cs** - Quản lý prefab theo loại exit
- [x] **DungeonRoomInstance.cs** - Đại diện phòng trong dungeon  
- [x] **DungeonGeneratorNew.cs** - Quản lý sinh tạo dungeon
- [x] **DungeonSetupHelper.cs** - Helper tool cho Editor
- [x] **README_NEW_SYSTEM.md** - Hướng dẫn sử dụng chi tiết
- [x] **PREFAB_ASSIGNMENT_GUIDE.md** - Danh sách prefab & cách gán
- [x] **SETUP_SUMMARY.md** - Tóm tắt quick-start
- [x] **SYSTEM_DIAGRAM.md** - Sơ đồ & quy trình chi tiết

## 📋 Cài đặt trong Unity

### Phase 1: Tạo GameObject & Component

- [ ] Mở scene hoặc tạo scene mới
- [ ] Chạy: Tools → Dungeon → Setup New Dungeon Generation System
  - [ ] Kiểm tra "RoomPrefabManager" được tạo
  - [ ] Kiểm tra "DungeonGenerator" được tạo
  - [ ] Kiểm tra DungeonGenerator có reference đến RoomPrefabManager

### Phase 2: Gán Prefab Phòng

#### Phòng loại Default
- [ ] RoomPrefabManager → Default Rooms → Size: 1
- [ ] Element 0:
  - [ ] Prefab: **DefaultRoom**
  - [ ] Room Type: **Default**
  - [ ] Room Size: **(12, 0, 12)**

#### Phòng loại Up  
- [ ] RoomPrefabManager → Up Rooms → Size: 1
- [ ] Element 0:
  - [ ] Prefab: **UpRoom**
  - [ ] Room Type: **Up**
  - [ ] Room Size: **(12, 0, 12)**

#### Phòng loại Down
- [ ] Có DownRoom.prefab?
  - [ ] YES → Gán vào Down Rooms list
  - [ ] NO → Tạo (copy từ DefaultRoom, edit)
    - [ ] Xóa tường Default
    - [ ] Thêm FloorTile vào DownWall
    - [ ] Rename thành DownRoom.prefab
    - [ ] Save
    - [ ] Gán vào Down Rooms list

#### Phòng loại Left
- [ ] RoomPrefabManager → Left Rooms → Size: 1
- [ ] Element 0:
  - [ ] Prefab: **LeftRoom**
  - [ ] Room Type: **Left**
  - [ ] Room Size: **(12, 0, 12)**

#### Phòng loại Right
- [ ] RoomPrefabManager → Right Rooms → Size: 1
- [ ] Element 0:
  - [ ] Prefab: **RightRoom**
  - [ ] Room Type: **Right**
  - [ ] Room Size: **(12, 0, 12)**

#### Phòng loại UpLeft
- [ ] RoomPrefabManager → UpLeft Rooms → Size: 1
- [ ] Element 0:
  - [ ] Prefab: **UpLeftRoom**
  - [ ] Room Type: **UpLeft**
  - [ ] Room Size: **(12, 0, 12)**

#### Phòng loại UpRight
- [ ] RoomPrefabManager → UpRight Rooms → Size: 1
- [ ] Element 0:
  - [ ] Prefab: **UpRightRoom**
  - [ ] Room Type: **UpRight**
  - [ ] Room Size: **(12, 0, 12)**

#### Phòng loại DownLeft (Tùy chọn)
- [ ] Có DownLeftRoom.prefab?
  - [ ] YES → Gán vào Down Left Rooms list
  - [ ] NO → Bỏ qua lúc này

#### Phòng loại DownRight (Tùy chọn)
- [ ] Có DownRightRoom.prefab?
  - [ ] YES → Gán vào Down Right Rooms list
  - [ ] NO → Bỏ qua lúc này

#### Các loại khác (Tùy chọn)
- [ ] UpDown Rooms - Bỏ qua lúc này
- [ ] LeftRight Rooms - Bỏ qua lúc này
- [ ] UpLeftRight Rooms - Bỏ qua lúc này
- [ ] DownLeftRight Rooms - Bỏ qua lúc này
- [ ] UpDownLeft Rooms - Bỏ qua lúc này
- [ ] UpDownRight Rooms - Bỏ qua lúc này
- [ ] AllDirections Rooms - Bỏ qua lúc này

### Phase 3: Cấu hình Generator

- [ ] Select "DungeonGenerator" (hoặc "DungeonGeneratorNew")
- [ ] Inspector:
  - [ ] Room Prefab Manager: **RoomPrefabManager** (drag từ Hierarchy)
  - [ ] Initial Room Position: **(0, 0, 0)**
  - [ ] Generation Speed: **0.2** (hoặc giá trị khác)
  - [ ] Max Rooms: **20** (số tối đa)
  - [ ] Min Rooms: **5** (số tối thiểu, chỉ thông tin)
  - [ ] Initial Room Type: **Up** hoặc **Default**

### Phase 4: Testing

- [ ] Play game
- [ ] Mở Console (Ctrl+Shift+C)
- [ ] Kiểm tra thông tin sinh tạo:
  ```
  === Dungeon Generation Info ===
  Total Rooms: X
  Total Connections: X
  === Room Types ===
  ...
  ```
- [ ] Pause game, check Hierarchy
  - [ ] DungeonGenerator có nhiều child room?
  - [ ] Các room có tên: Room_0_Up, Room_1_Down, etc.?
  - [ ] Các room có vị trí khác nhau?

### Phase 5: Debug & Tối ưu

- [ ] Nếu quá ít phòng:
  - [ ] Tăng Max Rooms
  - [ ] Giảm Generation Speed
  - [ ] Tăng tỷ lệ kết nối (80% → 90%) trong code

- [ ] Nếu quá nhiều phòng chồng lên nhau:
  - [ ] Giảm Max Rooms
  - [ ] Tăng tỷ lệ kết nối (80% → 60%)

- [ ] Nếu không có kết nối:
  - [ ] Kiểm tra Initial Room Type có exit không?
  - [ ] Kiểm tra các loại phòng tương thích đã gán?

## 🎮 Test Scenarios

### Test 1: Basic Generation
- [ ] Play game
- [ ] Dungeon sinh tạo thành công?
- [ ] Không có lỗi trong Console?

### Test 2: Multiple Types
- [ ] Generator tạo nhiều loại phòng khác nhau?
- [ ] Console hiển thị phân bố loại phòng?

### Test 3: No Overlaps
- [ ] Không có phòng chồng lên nhau?
- [ ] Chọn phòng, check Transform.position khác nhau?

### Test 4: Valid Connections
- [ ] Tất cả kết nối có exit tương thích?
- [ ] Up exit chỉ kết nối với Down exit?

### Test 5: Room Size
- [ ] Mỗi loại phòng có kích thước đúng (12,0,12)?
- [ ] Nếu thay đổi prefab, đã update Room Size?

## 📝 Tạo Prefab mới (Template)

Nếu cần tạo prefab mới (VD: DownRoom):

### Từ DefaultRoom
1. [ ] Mở Assets/Prefab/DefaultRoom.prefab
2. [ ] Ctrl+D (Duplicate) → Rename: DownRoom
3. [ ] Edit prefab:
   - [ ] DownWall → Xóa tất cả child
   - [ ] Drag FloorTile.prefab vào DownWall
   - [ ] Rename FloorTile thành WallWithHole
   - [ ] Adjust position/scale nếu cần
4. [ ] Save prefab
5. [ ] Gán vào list Down Rooms

### Scale đúng
```
Wall chiều cao: 2.2 unit
Floor dày: 0.2 unit
Phòng cơ bản: 11x11 unit
Padding: 1 unit
Total: 12x12 unit
```

## 🔍 Debugging Commands

Thêm vào script để debug:

```csharp
// Trong DungeonRoomInstance
public void PrintDebugInfo()
{
    Debug.Log($"Room Type: {roomType}");
    Debug.Log($"Room Size: {roomSize}");
    Debug.Log($"Room Position: {transform.position}");
    Debug.Log($"Bounds: {roomBounds}");
}

// Trong DungeonGeneratorNew
public void PrintAllRooms()
{
    Debug.Log($"Total Rooms: {allRooms.Count}");
    foreach(var room in allRooms)
    {
        Debug.Log($"  {room.name}: Type={room.GetRoomType()}, Pos={room.transform.position}");
    }
}
```

## 📊 Metrics sau sinh tạo

Kiểm tra những giá trị này:

| Metric | Tối thiểu | Lý tưởng | Tối đa |
|--------|-----------|---------|--------|
| Total Rooms | 5 | 10-15 | 20 |
| Total Connections | 4 | 8-12 | 19 |
| Unique Types | 2 | 4+ | 8+ |

## ✨ Hoàn thành!

Khi đã hoàn thành tất cả steps:

- [ ] Dungeon sinh tạo thành công
- [ ] Không có overlap
- [ ] Kết nối hợp lệ
- [ ] Console output hợp lý

🎉 **Hệ thống Dungeon Generation mới đã sẵn sàng!**

---

## 🆘 Gặp vấn đề?

### Không sinh tạo gì cả
- Kiểm tra Initial Room Type có exit
- Kiểm tra Room Prefab Manager được assign

### Quá ít phòng
- Tăng Max Rooms
- Giảm Generation Speed
- Tăng tỷ lệ kết nối

### Quá nhiều phòng chồng
- Giảm Max Rooms
- Kiểm tra Room Size có đúng
- Kiểm tra Overlap detection

### Lỗi reference
- Kiểm tra RoomPrefabManager được assign
- Kiểm tra prefab được drag vào list đúng

---

**Cần hỗ trợ thêm? Xem tài liệu:**
- README_NEW_SYSTEM.md
- PREFAB_ASSIGNMENT_GUIDE.md
- SYSTEM_DIAGRAM.md
