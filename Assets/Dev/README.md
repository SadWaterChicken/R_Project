# 🎮 Rapture 2D RPG - Dev Documentation

## 📁 Project Structure

```
Assets/Dev### 🔧 Key Scripts

### Core Scripts (Must Have):
- ✅ `PlayerData.cs` - Player stats and data
- ✅ `FirebaseManager.cs` - Database operations
- ✅ `GameManager.cs` - Game flow controller
- ✅ `SaveSlotData.cs` - Save slot structure
- ✅ `SaveSlotSelectionController.cs` - UI controller
- ✅ `MenuController.cs` - Main menu
- ✅ `SavePoint.cs` - In-game save points
- ✅ `PauseMenuController.cs` - ESC pause menu systemipts/          # All game scripts
├── Menu/            # Menu UI prefabs
├── Prefab/          # Game prefabs
├── Sprite/          # Sprites and assets
└── Documentation/   # Setup guides (this folder)
```

---

## 📚 Available Guides

### 1. **COMPLETE_SETUP_GUIDE.md** ⭐ MAIN GUIDE
Complete setup guide từ đầu đến cuối
- Firebase setup và configuration
- Core scripts overview
- 3-slot UI setup (current system)
- Testing scene setup
- Common issues và solutions
- Quick start checklist

### 2. **PAUSE_MENU_SETUP_GUIDE.md** ⭐ NEW
ESC Pause Menu setup guide
- ESC toggle pause menu
- Continue, Settings, Exit to Title buttons
- Auto-save on exit with confirmation
- UI setup và component configuration

### 3. **SAVE_TROUBLESHOOTING_GUIDE.md** 🔧 NEW
Debug guide for save/load issues
- Root cause analysis for Firebase sync issues
- Debug tools and testing workflow
- Common problems and solutions

### 4. **TESTING_SCENE_QUICK_SETUP.md**
Quick setup cho Testing scene
- Auto-setup tool (GameSceneChecker)
- Manual setup instructions
- Validation checklist

---

## 🎯 Core Systems

### Player Data System
**File:** `Scripts/PlayerData.cs`
- Health, Mana, Sanity mechanics
- Heal bằng sanity trade-off
- Level, Stats, Gold

### Firebase Save System
**Files:** 
- `Scripts/FirebaseManager.cs` - Database operations
- `Scripts/GameManager.cs` - Game flow management
- `Scripts/SaveSlotData.cs` - Data structure

**Features:**
- 3 save slots per user
- Cloud sync với Firebase Realtime Database
- Auto-load on game start
- Save point checkpoints

### Save Slot UI
**File:** `Scripts/SaveSlotSelectionController.cs`
- 3 slot selection screen
- Create new save / Load existing
- Progress bars and loading states
- Async operations với proper null checks

### Menu System
**File:** `Scripts/MenuController.cs`
- Play button → Show save slots
- Settings button
- Quit button

---

## 🚀 Quick Start

### First Time Setup:
1. Read **SETUP_GUIDE.md** for Firebase config
2. Follow **THREE_SAVE_SLOTS_UI_SETUP.md** for UI setup
3. Use **TESTING_SCENE_QUICK_SETUP.md** for testing scene

### Testing:
1. Open Menu scene
2. Click Play → See 3 save slots
3. Click empty slot → Create new save → Game starts
4. In game, find SavePoint → Press E to save
5. Back to menu → Click Play → See your save data

---

## 🔧 Key Scripts

### Core Scripts (Must Have):
- ✅ `PlayerData.cs` - Player stats and data
- ✅ `FirebaseManager.cs` - Database operations
- ✅ `GameManager.cs` - Game flow controller
- ✅ `SaveSlotData.cs` - Save slot structure
- ✅ `SaveSlotSelectionController.cs` - UI controller
- ✅ `MenuController.cs` - Main menu
- ✅ `SavePoint.cs` - In-game save points

### Editor Tools:
- ✅ `Editor/GameSceneChecker.cs` - Auto-setup tool

### Supporting Scripts:
- `PlayerMovement.cs` - Movement controller
- `PlayerUIController.cs` - UI display
- `SanityDebuffController.cs` - Sanity effects
- `PauseMenuController.cs` - ESC pause menu with auto-save
- `PauseMenuTester.cs` - Debug helper for pause system
- `SaveDebugger.cs` - Debug tool for save/load troubleshooting

---

## 🐛 Common Issues

### Save slots không hiển thị:
→ Check Firebase connection
→ Check SaveSlotSelectionController có assign đủ UI references

### MissingReferenceException:
→ Đã fix với null checks sau await
→ Check code có pattern: `if (this != null && gameObject != null)`

### Scene transition lag:
→ Async operations đã được optimize
→ Progress bars show feedback

### Duplicate save slots:
→ Đã fix bằng pre-created slots (không dùng Instantiate)

---

## 📝 Notes

### Sanity Mechanic:
- Player có thể heal bằng cách trade sanity
- Low sanity → Debuffs
- Unique healing system thay vì regen

### Save System:
- 3 slots cố định
- Firebase sync automatic
- Save points in-game only (không auto-save)

### UI Layout:
- Save slots: Top-right corner
- Main menu buttons: Center-left
- Clean and minimal design

---

## 🎨 Current Status

### ✅ Completed:
- Player data system with sanity
- Firebase integration
- 3 save slot system
- Save point checkpoints
- Async/await fixes
- UI layout

### 🔄 Ready for:
- Game content development
- More save points
- Combat system integration
- Sanity debuff effects

---

## 📞 Guide Navigation

**New to project?**
→ Start with `COMPLETE_SETUP_GUIDE.md` (all-in-one guide)

**Creating test scene?**
→ Check `TESTING_SCENE_QUICK_SETUP.md` (faster setup)

**Quick reference?**
→ Read this `README.md`

---

**Last Updated:** October 3, 2025
**Version:** 3-Slot System (Current)
**Status:** Production Ready ✅
