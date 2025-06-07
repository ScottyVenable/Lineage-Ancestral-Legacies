using System.Collections.Generic;
using UnityEngine;

namespace Lineage.Systems.Crafting
{
    [System.Serializable]
    public class CraftingRecipe
    {
        public string recipeId;
        public List<Ingredient> ingredients;
        public string resultItemId;
        public int resultQuantity;
    }

    [System.Serializable]
    public class Ingredient
    {
        public string itemId;
        public int quantity;
    }
}
