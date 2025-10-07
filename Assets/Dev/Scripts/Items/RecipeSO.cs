using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Ingredient
{
    public ItemSO item;
    public int amount;
}

[CreateAssetMenu(menuName = "RPG/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string recipeId;
    public List<Ingredient> ingredients = new List<Ingredient>();
    public int goldCost = 0;
    public ItemSO result;
    public string requiredQuestId; // optional
}
