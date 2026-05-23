using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dungeon Theme", menuName = "Dungeon Theme")]
public class DungeonTheme : ScriptableObject
{
    public string themeName;
    public enum Sin{Pride, Greed, Lust, Envy, Gluttony, Wrath, Sloth}
    public Sin sin;
    public List<GameObject> enemyPrefabs;
    public List<GameObject> bossPrefabs;
    

}