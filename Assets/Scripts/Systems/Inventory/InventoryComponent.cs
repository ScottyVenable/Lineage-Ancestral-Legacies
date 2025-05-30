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
            if (GetTotalItemCount() + count > capacity)
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

        /// <summary>
        /// Gets all items in the inventory.
        /// </summary>
        public Dictionary<string, int> GetAllItems()
        {
            return new Dictionary<string, int>(items);
        }

        /// <summary>
        /// Gets the total number of items in the inventory.
        /// </summary>
        public int GetTotalItemCount()
        {
            int total = 0;
            foreach (var item in items)
            {
                total += item.Value;
            }
            return total;
        }

        /// <summary>
        /// Checks if the inventory is full.
        /// </summary>
        public bool IsInventoryFull()
        {
            return GetTotalItemCount() >= capacity;
        }

        /// <summary>
        /// Clears all items from the inventory.
        /// </summary>
        public void ClearInventory()
        {
            items.Clear();
        }
    }
}
