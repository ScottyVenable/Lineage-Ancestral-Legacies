using System.Collections.Generic;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.Systems.Crafting
{
    [System.Serializable]
    public class CraftingRecipe
    {
        public string recipeId;
        public List<Ingredient> ingredients;
        public Lineage.Ancestral.Legacies.Database.Item.ID resultItemId;
        public int resultQuantity;
    }

    [System.Serializable]
    public class Ingredient
    {
        public Lineage.Ancestral.Legacies.Database.Item.ID itemId;
        public int quantity;
    }
}
