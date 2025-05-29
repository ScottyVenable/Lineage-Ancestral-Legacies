using System.Collections.Generic;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.Systems.Inventory
{
    /// <summary>
    /// Manages inventory items for a pop.
    /// </summary>
    public class InventoryComponent : MonoBehaviour
    {
        [Header("Inventory Settings")]
        public int capacity = 10;
        private Dictionary<string, int> items = new Dictionary<string, int>();

        /// <summary>
        /// Adds an item to the inventory. Returns true if successful.
        /// </summary>
        public bool AddItem(string itemId, int count = 1)
        {
            if (GetTotalItems() + count > capacity)
                return false;

            if (!items.ContainsKey(itemId))
                items[itemId] = 0;

            items[itemId] += count;
            return true;
        }

        /// <summary>
        /// Removes an item from the inventory. Returns true if successful.
        /// </summary>
        public bool RemoveItem(string itemId, int count = 1)
        {
            if (!items.ContainsKey(itemId) || items[itemId] < count)
                return false;

            items[itemId] -= count;
            if (items[itemId] <= 0)
                items.Remove(itemId);

            return true;
        }

        /// <summary>
        /// Gets quantity of a specific item.
        /// </summary>
        public int GetItemCount(string itemId)
        {
            if (items.ContainsKey(itemId))
                return items[itemId];
            return 0;
        }

        private int GetTotalItems()
        {
            int total = 0;
            foreach (var pair in items)
                total += pair.Value;
            return total;
        }
    }
}
