using UnityEngine;

namespace New_Dungeon
{
    [System.Serializable]
    public class BuffItemData : ItemData
    {
        public DungeonBuff buffToApply;

        public BuffItemData() : base() 
        { 
            // Mark as consumable/useable in inventory logic
        }

        public void UseBuff()
        {
            if (buffToApply != null && PlayerBuffManager.Instance != null)
            {
                PlayerBuffManager.Instance.ApplyBuff(buffToApply);
                Debug.Log($"[BuffItem] Used {itemName} to apply {buffToApply.buffName}");
            }
        }
    }
}
