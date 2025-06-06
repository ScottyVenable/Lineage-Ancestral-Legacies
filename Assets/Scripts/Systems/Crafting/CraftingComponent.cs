using System.Collections.Generic;
using UnityEngine;

namespace Lineage.Systems.Crafting
{
    /// <summary>
    /// Manages crafting logic and recipe execution.
    /// </summary>
    public class CraftingComponent : MonoBehaviour
    {
        // Loaded recipes
        public List<CraftingRecipe> availableRecipes;

        private Systems.Inventory.InventoryComponent inventory;

        private void Awake()
        {
            inventory = GetComponent<Systems.Inventory.InventoryComponent>();
        }

        /// <summary>
        /// Checks if this pop can craft the given recipe.
        /// </summary>
        public bool CanCraft(CraftingRecipe recipe)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                if (inventory.GetItemCount(ingredient.itemId) < ingredient.quantity)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Performs the crafting, consuming ingredients and producing result.
        /// </summary>
        public bool PerformCraft(CraftingRecipe recipe)
        {
            if (!CanCraft(recipe))
                return false;

            foreach (var ingredient in recipe.ingredients)
            {
                inventory.RemoveItem(ingredient.itemId, ingredient.quantity);
            }

            inventory.AddItem(recipe.resultItemId, recipe.resultQuantity);
            return true;
        }
    }
}
