# ⚡ Quick Reference - Interaction System

## 🎮 Player Input

```
┌─────────────────────────────────────────┐
│          PLAYER INPUTS                  │
├─────────────────────────────────────────┤
│ F  → Interact (Shop/Door/Portal)        │
│ I  → Toggle Inventory                   │
│ E  → Rotate Camera Right (if not near) │
│ Q  → Rotate Camera Left (if not near)  │
│ WASD → Move                             │
│ Shift → Sprint                          │
│ Space → Jump                            │
└─────────────────────────────────────────┘
```

---

## 🔧 For Developers

### Adding a New Interactable

**Step 1**: Create script
```csharp
using UnityEngine;

public class MyInteractable : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interacted with my object!");
        // Your logic here
    }
}
```

**Step 2**: Add to GameObject
- Add script
- Add Collider2D (or 3D Collider)
- Set "Is Trigger" = true

**Done!** PlayerController will auto-detect it.

---

### Debugging Interaction

```csharp
// In PlayerController.DetectNearbyInteractables():
Debug.Log($"Found interactable: {nearbyInteractable?.gameObject.name}");

// Or add to your IInteractable:
public void Interact()
{
    Debug.Log($"[{gameObject.name}] Interact called!");
}
```

---

## 📂 Key Files

| File | Purpose |
|------|---------|
| PlayerController.cs | Main input handler |
| IInteractable.cs | Interface for all interactables |
| ShopTrigger.cs | Shop system |
| InventoryInput.cs | Inventory toggle |
| Door.cs | Door system |
| DungeonPortal.cs | Portal system |

---

## 🐛 Common Issues

### "F doesn't work to open shop"
→ Check PlayerController.cs line 117: `if (inputActions.Player.Interact.triggered && nearbyInteractable != null)`

### "Shop opens even when far away"  
→ Check ShopTrigger has Collider2D with "Is Trigger" = true

### "Camera still rotates when near shop"
→ This was fixed! Check line 119-120 in PlayerController.cs for the nearbyInteractable check

### "Inventory can't be opened"
→ Check InventoryInput.cs line 40: `if (Input.GetKeyDown(KeyCode.I))`

---

## 💡 Design Philosophy

**One Input Source** = PlayerController
**One Interface** = IInteractable  
**One Key** = F for all interactions

This makes the system:
- ✅ Easy to understand
- ✅ Easy to extend
- ✅ Easy to debug
- ✅ No conflicts

---

## 🔗 Full Documentation

See: `INTERACTION_SYSTEM_DOCUMENTATION.md`

