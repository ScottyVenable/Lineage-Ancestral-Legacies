using System.Collections.Generic;
using UnityEngine;

namespace Lineage.Systems.Inventory
{
    /// <summary>
    /// Manages inventory items for a pop.
    /// </summary>
    public class InventoryComponent : MonoBehaviour
    {
        [Header("Inventory Settings")]
        public int capacity = 10;

        private Inventory _inventory;

        private void Awake()
        {
            _inventory = new Inventory(capacity);
        }

        /// <summary>
        /// Adds an item to the inventory. Returns true if successful.
        /// </summary>
        public bool AddItem(string itemId, int count = 1)
        {
            return _inventory.AddItem(itemId, count);
        }

        /// <summary>
        /// Removes an item from the inventory. Returns true if successful.
        /// </summary>
        public bool RemoveItem(string itemId, int count = 1)
        {
            return _inventory.RemoveItem(itemId, count);
        }

        /// <summary>
        /// Gets quantity of a specific item.
        /// </summary>
        public int GetItemCount(string itemId)
        {
            return _inventory.GetItemCount(itemId);
        }

        /// <summary>
        /// Gets all items in the inventory.
        /// </summary>
        public Dictionary<string, int> GetAllItems()
        {
            return _inventory.GetAllItems();
        }

        /// <summary>
        /// Gets the total number of items in the inventory.
        /// </summary>
        public int GetTotalItemCount()
        {
            return _inventory.GetTotalItemCount();
        }

        /// <summary>
        /// Checks if the inventory is full.
        /// </summary>
        public bool IsInventoryFull()
        {
            return _inventory.IsFull();
        }

        /// <summary>
        /// Clears all items from the inventory.
        /// </summary>
        public void ClearInventory()
        {
            _inventory.Clear();
        }

        public Inventory GetInventory() => _inventory;
    }
}
