using UnityEngine;

namespace New_Dungeon
{
    [CreateAssetMenu(fileName = "NewDungeonSack", menuName = "Dungeon/Dungeon Sack Data")]
    public class DungeonSackData : ScriptableObject
    {
        public string sackName = "Starter Dungeon Sack";
        public int capacity = 5;
        public string description = "A simple sack used to carry items found in the dungeon. Will be lost if you die.";
    }
}
