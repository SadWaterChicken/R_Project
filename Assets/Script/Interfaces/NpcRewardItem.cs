using UnityEngine;

public class NpcRewardItem : MonoBehaviour
{
    [Header("Item Setup")]

    public ItemData rewardItem;

    public void RewardItem()
    {
        if (rewardItem != null && !string.IsNullOrEmpty(rewardItem.itemID))
        {
            Inventory.Instance.AddItem(rewardItem);
            
        }
        else
        {
            Debug.LogWarning($"[Interactable] Quên chưa setup ItemData cho {gameObject.name} kìa bạn ơi!");
        }
    }
}
