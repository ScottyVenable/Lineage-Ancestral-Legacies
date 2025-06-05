using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Repository for managing item-related data.
    /// </summary>
    public static class ItemRepository
    {
        #region Database
        public static List<Item> AllItems { get; private set; } = new List<Item>();
        #endregion

        #region Item Methods
        /// <summary>
        /// Gets an item by its ID.
        /// </summary>
        /// <param name="itemID">The ID of the item to retrieve.</param>
        /// <returns>The item with the specified ID, or default if not found.</returns>
        public static Item GetItemByID(int itemID)
        {
            return AllItems.FirstOrDefault(i => i.itemID == itemID);
        }

        /// <summary>
        /// Gets items by their type.
        /// </summary>
        /// <param name="itemType">The type of items to retrieve.</param>
        /// <returns>A list of items of the specified type.</returns>
        public static List<Item> GetItemsByType(ItemType itemType)
        {
            return AllItems.Where(i => i.itemType == itemType).ToList();
        }

        /// <summary>
        /// Gets items by their rarity.
        /// </summary>
        /// <param name="rarity">The rarity level to filter by.</param>
        /// <returns>A list of items with the specified rarity.</returns>
        public static List<Item> GetItemsByRarity(ItemRarity rarity)
        {
            return AllItems.Where(i => i.itemRarity == rarity).ToList();
        }

        /// <summary>
        /// Gets items by their quality.
        /// </summary>
        /// <param name="quality">The quality level to filter by.</param>
        /// <returns>A list of items with the specified quality.</returns>
        public static List<Item> GetItemsByQuality(ItemQuality quality)
        {
            return AllItems.Where(i => i.itemQuality == quality).ToList();
        }

        /// <summary>
        /// Gets items that have a specific tag.
        /// </summary>
        /// <param name="tag">The tag to search for.</param>
        /// <returns>A list of items with the specified tag.</returns>
        public static List<Item> GetItemsByTag(string tag)
        {
            return AllItems.Where(i => i.tags != null && i.tags.Contains(tag)).ToList();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the item database with default data.
        /// </summary>
        public static void InitializeDatabase()
        {
            AllItems.Clear();
            
            // TODO: Add default item data initialization
            // Example item initialization
            var defaultItem = new Item
            {
                itemName = "Basic Sword",
                itemID = 1,
                itemType = ItemType.Weapon,
                weight = 2.5f,
                quantity = 1,
                value = 50,
                itemRarity = ItemRarity.Common,
                itemQuality = ItemQuality.Normal,
                tags = new List<string> { "Weapon", "Melee", "Metal" }
            };
            
            AllItems.Add(defaultItem);
            
            Debug.Log("ItemRepository: Database initialized successfully.");
        }
        #endregion
    }
}
