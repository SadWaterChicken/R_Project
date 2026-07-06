using UnityEngine;

public class InteractableItemReward : MonoBehaviour, IInteractable
{
    [Header("Item Setup")]
    [Tooltip("Tùy chỉnh thông số Item ngay tại đây (itemID, forgeLevel, modifiers...)")]
    public ItemData rewardItem;

    [Header("Settings")]
    [Tooltip("Có xóa object này sau khi nhặt đồ không?")]
    public bool destroyAfterInteract = true;

    // Hàm này sẽ được Player gọi khi ấn nút tương tác
    public void Interact()
    {
        if (rewardItem != null && !string.IsNullOrEmpty(rewardItem.itemID))
        {
            // 1. Quăng đồ vào túi người chơi
            Inventory.Instance.AddItem(rewardItem);
            
            Debug.Log($"[Interactable] Đã nhận phần thưởng: {rewardItem.itemID}");

            // 2. Xóa object khỏi map (nếu cần)
            if (destroyAfterInteract)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogWarning($"[Interactable] Quên chưa setup ItemData cho {gameObject.name} kìa bạn ơi!");
        }
    }
}
