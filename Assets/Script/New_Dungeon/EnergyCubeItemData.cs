using UnityEngine;

namespace New_Dungeon
{
    [System.Serializable]
    public class EnergyCubeItemData : ItemData
    {
        public int cubeValue = 1;

        public EnergyCubeItemData() : base()
        {
            itemID = "item_energy_cube";
            itemName = "Energy Cube";
            description = "A mysterious cube pulsating with energy. Can be offered at an Event Structure.";
        }
    }
}
