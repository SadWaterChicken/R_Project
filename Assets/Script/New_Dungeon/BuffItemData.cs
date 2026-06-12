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
            if (buffToApply != null && PlayerStat.Instance != null)
            {
                CharacterBuffManager buffManager = PlayerStat.Instance.GetComponent<CharacterBuffManager>();
                if (buffManager != null)
                {
                    buffManager.ApplyBuff(buffToApply);
                    Debug.Log($"[BuffItem] Used {itemName} to apply {buffToApply.buffName}");
                }
            }
        }
    }
}
