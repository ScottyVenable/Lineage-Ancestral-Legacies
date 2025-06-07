using System.Collections.Generic;
using UnityEngine;

namespace Lineage.Core.Crafting
{
    /// <summary>
    /// Definition for a crafting recipe.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipeDef", menuName = "GameData/Recipes/Recipe Definition")]
    public class RecipeDefinitionSO : Lineage.Core.GameDataSO
    {
        [System.Serializable]
        public class Ingredient
        {
            public ItemDefinitionSO itemDefinition;
            public int quantity;
        }

        public List<Ingredient> ingredients = new List<Ingredient>();
        public ItemDefinitionSO outputItem;
        public int outputQuantity = 1;
        public float craftingTimeSeconds;
    }
}
