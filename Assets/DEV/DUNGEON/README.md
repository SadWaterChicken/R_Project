# 🏰 Hệ thống Sinh tạo Dungeon Mới - Tài liệu Chính

**Ngày tạo:** 2026-02-02  
**Phiên bản:** 1.0  
**Trạng thái:** Sẵn sàng sử dụng  

---

## 📖 Giới thiệu

Hệ thống Dungeon Generation mới được thiết kế để:
- ✅ Quản lý prefab phòng theo hướng exit (Up, Down, Left, Right)
- ✅ Tự động kết nối phòng hợp lệ (exit đối nhau)
- ✅ Kiểm tra overlap trước khi tạo phòng
- ✅ Dễ mở rộng với các loại phòng mới
- ✅ Cấu hình dễ dàng qua Inspector

## 🚀 Bắt đầu nhanh (5 phút)

### Bước 1: Setup tự động
```
Trong Unity Editor:
Tools → Dungeon → Setup New Dungeon Generation System
```

### Bước 2: Gán prefab
Xem hướng dẫn trong **PREFAB_ASSIGNMENT_GUIDE.md**

### Bước 3: Play
```
Press Play → Dungeon sinh tạo tự động
Console → Xem thông tin sinh tạo
```

## 📚 Tài liệu Chi tiết

| File | Nội dung |
|------|---------|
| **SETUP_SUMMARY.md** | 📋 Tóm tắt setup & ý tưởng chính |
| **README_NEW_SYSTEM.md** | 📖 Hướng dẫn sử dụng chi tiết |
| **PREFAB_ASSIGNMENT_GUIDE.md** | 🎨 Danh sách prefab & cách gán |
| **SYSTEM_DIAGRAM.md** | 🎯 Sơ đồ kiến trúc & flow |
| **SETUP_CHECKLIST.md** | ✅ Checklist setup từng bước |
| **IMPLEMENTATION_NOTES.md** | 📝 Quick reference & notes |

## 🏗️ Kiến trúc Hệ thống

```
RoomPrefabManager
    ↓ (cung cấp prefab)
DungeonGeneratorNew
    ↓ (spawn)
DungeonRoomInstance × N
```

**Luồng dữ liệu:**
```
Queue: [Room1] → [Room2] → [Room3] → ...
         ↓
    Kiểm tra 4 hướng
         ↓
    Tìm exit & loại tương thích
         ↓
    Kiểm tra overlap
         ↓
    Spawn phòng mới
```

## 🎯 Các thành phần

### 1. RoomPrefabManager.cs (170 dòng)
Quản lý tất cả prefab phòng theo loại exit

**Công khai:**
- `GetRandomRoomPrefab(RoomType)` - Lấy prefab ngẫu nhiên
- `GetCompatibleRoomType(Direction)` - Loại phòng tương thích
- `HasExitInDirection(RoomType, Direction)` - Kiểm tra exit

### 2. DungeonRoomInstance.cs (60 dòng)
Đại diện cho một phòng được spawn

**Công khai:**
- `Initialize(type, size, generator)` - Khởi tạo phòng
- `GetRoomType()` - Lấy loại
- `GetAdjacentRoomPosition(direction)` - Vị trí phòng kế tiếp

### 3. DungeonGeneratorNew.cs (270 dòng)
Quản lý quá trình sinh tạo dungeon

**Quy trình:**
1. Spawn phòng ban đầu
2. Lặp xử lý từng phòng
3. Kiểm tra 4 hướng
4. Spawn phòng tương thích (nếu hợp lệ)
5. Tiếp tục đến hết queue

### 4. DungeonSetupHelper.cs (30 dòng)
Công cụ trợ giúp trong Editor

**Menu:**
- `Dungeon → Setup New Dungeon Generation System`
- `Dungeon → Find Room Prefabs`

## 🎲 Ví dụ Sinh tạo

### Ví dụ 1: Tuyến tính (Linear)
```
UpRoom ─→ DownRoom ─→ UpRoom ─→ DownRoom
  ↓         ↓           ↓         ↓
(0,0,0)  (0,0,12)   (0,0,24)  (0,0,36)
```

### Ví dụ 2: Nhánh rẽ (Branching)
```
        ┌─→ [RightRoom]
[UpLeftRightRoom] ←─
        └─→ [LeftRoom]
```

### Ví dụ 3: Lưới (Grid)
```
[UpLeft] ──→ [UpRight]
   ↓ ↓         ↓ ↓
[DownLeft] ──→ [DownRight]
```

## 📦 Prefab cần gán

**Bắt buộc (dùng ngay):**
- ✅ DefaultRoom.prefab
- ✅ UpRoom.prefab
- ✅ LeftRoom.prefab
- ✅ RightRoom.prefab
- ✅ UpLeftRoom.prefab
- ✅ UpRightRoom.prefab

**Tùy chọn (tạo sau):**
- ⚠️ DownRoom.prefab (copy từ DefaultRoom)
- ⚠️ DownLeftRoom.prefab
- ⚠️ DownRightRoom.prefab

Xem **PREFAB_ASSIGNMENT_GUIDE.md** để chi tiết.

## 🔄 Quy tắc Kết nối

### Mối Quan hệ Exit
```
Phòng A.Exit[Up] ←→ Phòng B.Exit[Down]
Phòng A.Exit[Down] ←→ Phòng B.Exit[Up]
Phòng A.Exit[Left] ←→ Phòng B.Exit[Right]
Phòng A.Exit[Right] ←→ Phòng B.Exit[Left]
```

### Vị Trí Phòng Mới
```
A.Exit[Up] (Z+)
    ↓
B.position = A.position + (0, 0, 12)
    ↓
B loại phải là: Down
```

## 🛡️ Kiểm tra Overlap

Trước khi spawn phòng mới:
1. Tính bounds của phòng mới
2. So sánh với bounds tất cả phòng cũ
3. Nếu giao nhau → INVALID
4. Nếu không → VALID → Spawn

```csharp
newBounds.Intersects(existingRoom.GetBounds())
```

## ⚙️ Cấu hình

**Tệp:** RoomPrefabManager (trong Inspector)
```
[Default Rooms]
  Element 0: DefaultRoom.prefab

[Up Rooms]
  Element 0: UpRoom.prefab

[Down Rooms]
  Element 0: DownRoom.prefab
  
... (tương tự cho các loại khác)
```

**Tệp:** DungeonGeneratorNew (trong Inspector)
```
Room Prefab Manager: [Drag RoomPrefabManager]
Initial Room Position: (0, 0, 0)
Generation Speed: 0.2
Max Rooms: 20
Min Rooms: 5
Initial Room Type: Up
```

## 🎮 Sử dụng

### Code
```csharp
// Lấy đối tượng generator
DungeonGeneratorNew generator = GetComponent<DungeonGeneratorNew>();

// Bắt đầu sinh tạo (tự động trong Start)
generator.StartDungeonGeneration();

// Lấy tất cả phòng
HashSet<DungeonRoomInstance> rooms = generator.GetAllRooms();

// Lấy kết nối
List<(DungeonRoomInstance, Direction, DungeonRoomInstance)> connections 
    = generator.GetConnections();
```

### Inspector
1. Chọn "DungeonGenerator" trong Hierarchy
2. Cấu hình tham số
3. Play game

## 📊 Output

**Console sau sinh tạo:**
```
=== Dungeon Generation Info ===
Total Rooms: 15
Total Connections: 14
=== Room Types ===
Default: 2
Up: 5
Down: 3
Left: 2
Right: 2
UpLeft: 1
```

**Hierarchy:**
```
DungeonGenerator
├── Room_0_Up
├── Room_1_Down
├── Room_2_Up
├── Room_3_LeftRight
└── ...
```

## 🔧 Tùy chỉnh

### Tỷ lệ Kết nối (80%)
**File:** DungeonGeneratorNew.cs, dòng 78
```csharp
if (Random.value < 0.8f)  // Thay 0.8f
```

### Generation Speed (0.2s)
**Inspector:** DungeonGenerator → Generation Speed

### Max Rooms (20)
**Inspector:** DungeonGenerator → Max Rooms

### Initial Room Type
**Inspector:** DungeonGenerator → Initial Room Type
(Gợi ý: Dùng Up thay vì Default để có kết nối)

## 🚀 Mở rộng

### Thêm loại phòng mới

**Bước 1:** Tạo prefab
```
Duplicate DefaultRoom → DownRoom
Edit: DownWall thêm FloorTile
Save
```

**Bước 2:** Gán vào list
```
RoomPrefabManager → Down Rooms → Add Element
Prefab: DownRoom
Type: Down
Size: (12, 0, 12)
```

**Bước 3:** Cài đặt xong!

### Thêm logic phức tạp
- Override `Initialize()` trong DungeonRoomInstance
- Thêm component khác cho phòng
- Thêm trigger cho doorway

## 📈 Performance

| Aspect | Giá trị |
|--------|--------|
| Max rooms | 20 |
| Overlap check | O(1) per room |
| Total complexity | O(n²) |
| Thời gian sinh (20 rooms) | ~4 giây |

## 🐛 Debug

### Không sinh tạo
1. Kiểm tra Initial Room Type có exit
2. Kiểm tra RoomPrefabManager được assign
3. Kiểm tra prefab được gán vào list

### Quá ít phòng
1. Tăng Max Rooms
2. Giảm Generation Speed
3. Tăng tỷ lệ kết nối

### Phòng chồng nhau
1. Giảm Max Rooms
2. Kiểm tra Room Size đúng
3. Kiểm tra Overlap detection

## 📋 Checklist

- [ ] Setup RoomPrefabManager + DungeonGenerator
- [ ] Gán tất cả prefab cần thiết
- [ ] Play & kiểm tra output
- [ ] Tùy chỉnh tỷ lệ nếu cần
- [ ] Thêm logic doorway (sau)

## 🎓 Học thêm

Xem các tài liệu:
- `SETUP_SUMMARY.md` - Tóm tắt nhanh
- `README_NEW_SYSTEM.md` - Chi tiết đầy đủ
- `SYSTEM_DIAGRAM.md` - Sơ đồ & flow
- `SETUP_CHECKLIST.md` - Từng bước setup

## 📞 Hỗ trợ

**Gặp vấn đề?** Xem:
1. SETUP_CHECKLIST.md → "Gặp vấn đề?"
2. SYSTEM_DIAGRAM.md → "Ví dụ cụ thể"
3. Console → Kiểm tra lỗi

## 📄 License

Miễn phí sử dụng trong dự án.

---

## 🎉 Tóm tắt

**Hệ thống Dungeon Generation mới:**
- ✅ Quản lý prefab theo exit direction
- ✅ Tự động kết nối phòng hợp lệ
- ✅ Kiểm tra overlap chống chồng
- ✅ Dễ mở rộng & tùy chỉnh
- ✅ Code sạch & có tài liệu

**Sẵn sàng sinh tạo dungeon một cách thông minh! 🏰**

---

*Để bắt đầu, chạy: `Tools → Dungeon → Setup New Dungeon Generation System`*
