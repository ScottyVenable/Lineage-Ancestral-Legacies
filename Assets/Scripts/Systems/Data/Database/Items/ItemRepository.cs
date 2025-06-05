using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Static repository class for managing items in the game database.
    /// Provides CRUD operations, filtering, and inventory management functionality.
    /// </summary>
    public static class ItemRepository
    {
        #region Private Fields

        private static Dictionary<int, Item> _items = new Dictionary<int, Item>();
        private static Dictionary<string, int> _itemNameToID = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<ItemType, List<int>> _itemsByType = new Dictionary<ItemType, List<int>>();
        private static Dictionary<ItemRarity, List<int>> _itemsByRarity = new Dictionary<ItemRarity, List<int>>();
        private static Dictionary<string, List<int>> _itemsByTag = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
        private static int _nextItemID = 1;
        private static bool _isInitialized = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the total number of unique items in the repository.
        /// </summary>
        public static int TotalItems => _items.Count;

        /// <summary>
        /// Gets all item IDs currently in the repository.
        /// </summary>
        public static IEnumerable<int> AllItemIDs => _items.Keys;

        /// <summary>
        /// Gets whether the repository has been initialized.
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the item repository with default data.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            ClearAll();
            InitializeDefaultItems();
            _isInitialized = true;

            Debug.Log($"ItemRepository initialized with {_items.Count} items.");
        }

        /// <summary>
        /// Creates default items for the game.
        /// </summary>
        private static void InitializeDefaultItems()
        {
            // Basic Weapons
            CreateItem("Iron Sword", ItemType.Weapon, ItemRarity.Common, 3f, 150, 1, "A sturdy iron sword.");
            CreateItem("Steel Sword", ItemType.Weapon, ItemRarity.Uncommon, 3.5f, 300, 1, "A well-crafted steel sword.");
            CreateItem("Enchanted Blade", ItemType.Weapon, ItemRarity.Rare, 4f, 1000, 1, "A magically enhanced blade.");
            CreateItem("Dragon Slayer", ItemType.Weapon, ItemRarity.Legendary, 5f, 5000, 1, "A legendary sword forged to slay dragons.");

            // Basic Armor
            CreateItem("Leather Armor", ItemType.Armor, ItemRarity.Common, 8f, 100, 1, "Basic leather protection.");
            CreateItem("Chain Mail", ItemType.Armor, ItemRarity.Uncommon, 15f, 250, 1, "Interlocked metal rings for protection.");
            CreateItem("Plate Armor", ItemType.Armor, ItemRarity.Rare, 25f, 800, 1, "Heavy metal plate armor.");
            CreateItem("Dragon Scale Armor", ItemType.Armor, ItemRarity.Epic, 20f, 3000, 1, "Armor crafted from dragon scales.");

            // Consumables
            CreateItem("Health Potion", ItemType.Consumable, ItemRarity.Common, 0.2f, 25, 10, "Restores 50 health points.");
            CreateItem("Mana Potion", ItemType.Consumable, ItemRarity.Common, 0.2f, 30, 10, "Restores 50 mana points.");
            CreateItem("Greater Health Potion", ItemType.Consumable, ItemRarity.Uncommon, 0.3f, 75, 5, "Restores 150 health points.");
            CreateItem("Elixir of Strength", ItemType.Consumable, ItemRarity.Rare, 0.1f, 200, 3, "Temporarily increases strength.");

            // Materials
            CreateItem("Iron Ore", ItemType.Material, ItemRarity.Common, 2f, 10, 50, "Raw iron ore for smelting.");
            CreateItem("Precious Gems", ItemType.Material, ItemRarity.Rare, 0.1f, 100, 20, "Beautiful gems for crafting.");
            CreateItem("Dragon Bone", ItemType.Material, ItemRarity.Epic, 5f, 500, 5, "Bones from an ancient dragon.");
            CreateItem("Moonstone", ItemType.Material, ItemRarity.Legendary, 0.5f, 1000, 3, "A mystical stone that glows with moonlight.");

            // Tools
            CreateItem("Pickaxe", ItemType.Tool, ItemRarity.Common, 4f, 50, 1, "Used for mining ores and stones.");
            CreateItem("Fishing Rod", ItemType.Tool, ItemRarity.Common, 1.5f, 30, 1, "Used for catching fish.");
            CreateItem("Master's Hammer", ItemType.Tool, ItemRarity.Rare, 6f, 300, 1, "A craftsman's finest tool.");

            // Currency
            CreateItem("Gold Coin", ItemType.Currency, ItemRarity.Common, 0.01f, 1, 1000, "Standard currency.");
            CreateItem("Silver Coin", ItemType.Currency, ItemRarity.Common, 0.01f, 0, 1000, "Lesser currency, 10 = 1 gold.");
            CreateItem("Platinum Coin", ItemType.Currency, ItemRarity.Uncommon, 0.01f, 10, 100, "Premium currency, 1 = 10 gold.");

            // Quest Items
            CreateItem("Ancient Key", ItemType.QuestItem, ItemRarity.Rare, 0.5f, 0, 1, "A mysterious key to an ancient door.");
            CreateItem("Royal Seal", ItemType.QuestItem, ItemRarity.Epic, 0.3f, 0, 1, "The official seal of the kingdom.");

            // Books
            CreateItem("Basic Spellbook", ItemType.Book, ItemRarity.Common, 1f, 50, 1, "Contains basic magical knowledge.");
            CreateItem("Tome of Ancient Wisdom", ItemType.Book, ItemRarity.Legendary, 2f, 2000, 1, "Contains powerful ancient secrets.");

            // Accessories
            CreateItem("Silver Ring", ItemType.Accessory, ItemRarity.Common, 0.1f, 75, 1, "A simple silver ring.");
            CreateItem("Ring of Power", ItemType.Accessory, ItemRarity.Epic, 0.1f, 1500, 1, "Increases magical abilities.");

            // Add tags to appropriate items
            AddTagsToItems();
        }

        /// <summary>
        /// Adds appropriate tags to created items.
        /// </summary>
        private static void AddTagsToItems()
        {
            // Add weapon tags
            AddTagToItemByName("Iron Sword", "melee");
            AddTagToItemByName("Iron Sword", "one-handed");
            AddTagToItemByName("Steel Sword", "melee");
            AddTagToItemByName("Steel Sword", "one-handed");
            AddTagToItemByName("Enchanted Blade", "melee");
            AddTagToItemByName("Enchanted Blade", "magical");
            AddTagToItemByName("Dragon Slayer", "melee");
            AddTagToItemByName("Dragon Slayer", "legendary");
            AddTagToItemByName("Dragon Slayer", "anti-dragon");

            // Add armor tags
            AddTagToItemByName("Leather Armor", "light");
            AddTagToItemByName("Chain Mail", "medium");
            AddTagToItemByName("Plate Armor", "heavy");
            AddTagToItemByName("Dragon Scale Armor", "magical");
            AddTagToItemByName("Dragon Scale Armor", "fire-resistant");

            // Add consumable tags
            AddTagToItemByName("Health Potion", "healing");
            AddTagToItemByName("Mana Potion", "mana-restore");
            AddTagToItemByName("Greater Health Potion", "healing");
            AddTagToItemByName("Elixir of Strength", "buff");
            AddTagToItemByName("Elixir of Strength", "temporary");

            // Add material tags
            AddTagToItemByName("Iron Ore", "smithing");
            AddTagToItemByName("Precious Gems", "jewelry");
            AddTagToItemByName("Dragon Bone", "legendary-crafting");
            AddTagToItemByName("Moonstone", "enchanting");

            // Add tool tags
            AddTagToItemByName("Pickaxe", "mining");
            AddTagToItemByName("Fishing Rod", "fishing");
            AddTagToItemByName("Master's Hammer", "crafting");
            AddTagToItemByName("Master's Hammer", "masterwork");
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new item and adds it to the repository.
        /// </summary>
        /// <param name="itemName">Name of the item</param>
        /// <param name="itemType">Type of the item</param>
        /// <param name="rarity">Rarity of the item</param>
        /// <param name="weight">Weight per unit</param>
        /// <param name="baseValue">Base value of the item</param>
        /// <param name="maxStackSize">Maximum stack size</param>
        /// <param name="description">Item description</param>
        /// <returns>The created item, or default if creation failed</returns>
        public static Item CreateItem(string itemName, ItemType itemType, ItemRarity rarity = ItemRarity.Common, 
            float weight = 1f, int baseValue = 1, int maxStackSize = 1, string description = "")
        {
            if (string.IsNullOrEmpty(itemName))
            {
                Debug.LogError("Cannot create item with null or empty name.");
                return default;
            }

            if (_itemNameToID.ContainsKey(itemName))
            {
                Debug.LogWarning($"Item with name '{itemName}' already exists.");
                return GetItemByName(itemName);
            }

            int itemID = _nextItemID++;
            var item = new Item(itemID, itemName, itemType, rarity);
            
            // Set additional properties through reflection or create a more comprehensive constructor
            // For now, we'll create the item and then modify its properties
            _items[itemID] = item;
            _itemNameToID[itemName] = itemID;

            // Add to type index
            if (!_itemsByType.ContainsKey(itemType))
                _itemsByType[itemType] = new List<int>();
            _itemsByType[itemType].Add(itemID);

            // Add to rarity index
            if (!_itemsByRarity.ContainsKey(rarity))
                _itemsByRarity[rarity] = new List<int>();
            _itemsByRarity[rarity].Add(itemID);

            Debug.Log($"Created item: {itemName} (ID: {itemID})");
            return item;
        }

        /// <summary>
        /// Gets an item by its ID.
        /// </summary>
        /// <param name="itemID">ID of the item to retrieve</param>
        /// <returns>The item if found, otherwise default</returns>
        public static Item GetItem(int itemID)
        {
            return _items.TryGetValue(itemID, out Item item) ? item : default;
        }

        /// <summary>
        /// Gets an item by its name.
        /// </summary>
        /// <param name="itemName">Name of the item to retrieve</param>
        /// <returns>The item if found, otherwise default</returns>
        public static Item GetItemByName(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return default;

            return _itemNameToID.TryGetValue(itemName, out int itemID) ? GetItem(itemID) : default;
        }

        /// <summary>
        /// Updates an existing item in the repository.
        /// </summary>
        /// <param name="updatedItem">The updated item</param>
        /// <returns>True if update was successful</returns>
        public static bool UpdateItem(Item updatedItem)
        {
            if (!_items.ContainsKey(updatedItem.ItemID))
            {
                Debug.LogError($"Cannot update item with ID {updatedItem.ItemID} - item not found.");
                return false;
            }

            Item oldItem = _items[updatedItem.ItemID];
            
            // Update name mapping if name changed
            if (oldItem.ItemName != updatedItem.ItemName)
            {
                _itemNameToID.Remove(oldItem.ItemName);
                _itemNameToID[updatedItem.ItemName] = updatedItem.ItemID;
            }

            // Update type mapping if type changed
            if (oldItem.ItemType != updatedItem.ItemType)
            {
                _itemsByType[oldItem.ItemType]?.Remove(updatedItem.ItemID);
                if (!_itemsByType.ContainsKey(updatedItem.ItemType))
                    _itemsByType[updatedItem.ItemType] = new List<int>();
                _itemsByType[updatedItem.ItemType].Add(updatedItem.ItemID);
            }

            // Update rarity mapping if rarity changed
            if (oldItem.ItemRarity != updatedItem.ItemRarity)
            {
                _itemsByRarity[oldItem.ItemRarity]?.Remove(updatedItem.ItemID);
                if (!_itemsByRarity.ContainsKey(updatedItem.ItemRarity))
                    _itemsByRarity[updatedItem.ItemRarity] = new List<int>();
                _itemsByRarity[updatedItem.ItemRarity].Add(updatedItem.ItemID);
            }

            // Update tag mappings
            UpdateTagMappings(oldItem, updatedItem);

            _items[updatedItem.ItemID] = updatedItem;
            return true;
        }

        /// <summary>
        /// Deletes an item from the repository.
        /// </summary>
        /// <param name="itemID">ID of the item to delete</param>
        /// <returns>True if deletion was successful</returns>
        public static bool DeleteItem(int itemID)
        {
            if (!_items.TryGetValue(itemID, out Item item))
            {
                Debug.LogError($"Cannot delete item with ID {itemID} - item not found.");
                return false;
            }

            // Remove from all mappings
            _itemNameToID.Remove(item.ItemName);
            _itemsByType[item.ItemType]?.Remove(itemID);
            _itemsByRarity[item.ItemRarity]?.Remove(itemID);

            // Remove from tag mappings
            foreach (string tag in item.Tags)
            {
                _itemsByTag[tag]?.Remove(itemID);
                if (_itemsByTag[tag]?.Count == 0)
                    _itemsByTag.Remove(tag);
            }

            _items.Remove(itemID);
            Debug.Log($"Deleted item: {item.ItemName} (ID: {itemID})");
            return true;
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Gets all items of a specific type.
        /// </summary>
        /// <param name="itemType">Type of items to retrieve</param>
        /// <returns>List of items of the specified type</returns>
        public static List<Item> GetItemsByType(ItemType itemType)
        {
            if (!_itemsByType.TryGetValue(itemType, out List<int> itemIDs))
                return new List<Item>();

            return itemIDs.Select(GetItem).Where(item => item.ItemID != 0).ToList();
        }

        /// <summary>
        /// Gets all items of a specific rarity.
        /// </summary>
        /// <param name="rarity">Rarity of items to retrieve</param>
        /// <returns>List of items of the specified rarity</returns>
        public static List<Item> GetItemsByRarity(ItemRarity rarity)
        {
            if (!_itemsByRarity.TryGetValue(rarity, out List<int> itemIDs))
                return new List<Item>();

            return itemIDs.Select(GetItem).Where(item => item.ItemID != 0).ToList();
        }

        /// <summary>
        /// Gets all items that have a specific tag.
        /// </summary>
        /// <param name="tag">Tag to search for</param>
        /// <returns>List of items with the specified tag</returns>
        public static List<Item> GetItemsByTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || !_itemsByTag.TryGetValue(tag, out List<int> itemIDs))
                return new List<Item>();

            return itemIDs.Select(GetItem).Where(item => item.ItemID != 0).ToList();
        }

        /// <summary>
        /// Searches for items by name (case-insensitive partial match).
        /// </summary>
        /// <param name="searchTerm">Term to search for in item names</param>
        /// <returns>List of items matching the search term</returns>
        public static List<Item> SearchItemsByName(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return new List<Item>();

            return _items.Values
                .Where(item => item.ItemName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        /// <summary>
        /// Gets all items within a specific value range.
        /// </summary>
        /// <param name="minValue">Minimum value</param>
        /// <param name="maxValue">Maximum value</param>
        /// <returns>List of items within the value range</returns>
        public static List<Item> GetItemsByValueRange(int minValue, int maxValue)
        {
            return _items.Values
                .Where(item => item.CurrentValue >= minValue && item.CurrentValue <= maxValue)
                .ToList();
        }

        /// <summary>
        /// Gets all consumable items.
        /// </summary>
        /// <returns>List of consumable items</returns>
        public static List<Item> GetConsumableItems()
        {
            return _items.Values.Where(item => item.IsConsumable).ToList();
        }

        /// <summary>
        /// Gets all quest items.
        /// </summary>
        /// <returns>List of quest items</returns>
        public static List<Item> GetQuestItems()
        {
            return _items.Values.Where(item => item.IsQuestItem).ToList();
        }

        /// <summary>
        /// Gets all tradeable items (not trade-restricted).
        /// </summary>
        /// <returns>List of tradeable items</returns>
        public static List<Item> GetTradeableItems()
        {
            return _items.Values.Where(item => !item.IsTradeRestricted).ToList();
        }

        #endregion

        #region Tag Management

        /// <summary>
        /// Adds a tag to an item by ID.
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        /// <param name="tag">Tag to add</param>
        /// <returns>True if tag was added successfully</returns>
        public static bool AddTagToItem(int itemID, string tag)
        {
            if (!_items.TryGetValue(itemID, out Item item) || string.IsNullOrEmpty(tag))
                return false;

            item.AddTag(tag);
            UpdateItem(item);

            // Update tag mapping
            if (!_itemsByTag.ContainsKey(tag))
                _itemsByTag[tag] = new List<int>();
            
            if (!_itemsByTag[tag].Contains(itemID))
                _itemsByTag[tag].Add(itemID);

            return true;
        }

        /// <summary>
        /// Adds a tag to an item by name.
        /// </summary>
        /// <param name="itemName">Name of the item</param>
        /// <param name="tag">Tag to add</param>
        /// <returns>True if tag was added successfully</returns>
        public static bool AddTagToItemByName(string itemName, string tag)
        {
            if (!_itemNameToID.TryGetValue(itemName, out int itemID))
                return false;

            return AddTagToItem(itemID, tag);
        }

        /// <summary>
        /// Removes a tag from an item by ID.
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        /// <param name="tag">Tag to remove</param>
        /// <returns>True if tag was removed successfully</returns>
        public static bool RemoveTagFromItem(int itemID, string tag)
        {
            if (!_items.TryGetValue(itemID, out Item item) || string.IsNullOrEmpty(tag))
                return false;

            bool removed = item.RemoveTag(tag);
            if (removed)
            {
                UpdateItem(item);
                
                // Update tag mapping
                _itemsByTag[tag]?.Remove(itemID);
                if (_itemsByTag[tag]?.Count == 0)
                    _itemsByTag.Remove(tag);
            }

            return removed;
        }

        /// <summary>
        /// Gets all unique tags in the repository.
        /// </summary>
        /// <returns>List of all tags</returns>
        public static List<string> GetAllTags()
        {
            return _itemsByTag.Keys.ToList();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Checks if an item with the given ID exists.
        /// </summary>
        /// <param name="itemID">ID to check</param>
        /// <returns>True if item exists</returns>
        public static bool ItemExists(int itemID)
        {
            return _items.ContainsKey(itemID);
        }

        /// <summary>
        /// Checks if an item with the given name exists.
        /// </summary>
        /// <param name="itemName">Name to check</param>
        /// <returns>True if item exists</returns>
        public static bool ItemExistsByName(string itemName)
        {
            return !string.IsNullOrEmpty(itemName) && _itemNameToID.ContainsKey(itemName);
        }

        /// <summary>
        /// Gets the next available item ID.
        /// </summary>
        /// <returns>Next item ID</returns>
        public static int GetNextItemID()
        {
            return _nextItemID;
        }

        /// <summary>
        /// Gets all items in the repository.
        /// </summary>
        /// <returns>List of all items</returns>
        public static List<Item> GetAllItems()
        {
            return _items.Values.ToList();
        }

        /// <summary>
        /// Clears all items from the repository.
        /// </summary>
        public static void ClearAll()
        {
            _items.Clear();
            _itemNameToID.Clear();
            _itemsByType.Clear();
            _itemsByRarity.Clear();
            _itemsByTag.Clear();
            _nextItemID = 1;
            _isInitialized = false;
        }

        /// <summary>
        /// Gets repository statistics.
        /// </summary>
        /// <returns>Formatted statistics string</returns>
        public static string GetRepositoryStats()
        {
            var stats = $"=== Item Repository Statistics ===\n" +
                       $"Total Items: {_items.Count}\n" +
                       $"Total Tags: {_itemsByTag.Count}\n" +
                       $"Items by Type:\n";

            foreach (var kvp in _itemsByType.Where(kvp => kvp.Value.Count > 0))
            {
                stats += $"  {kvp.Key}: {kvp.Value.Count}\n";
            }

            stats += "Items by Rarity:\n";
            foreach (var kvp in _itemsByRarity.Where(kvp => kvp.Value.Count > 0))
            {
                stats += $"  {kvp.Key}: {kvp.Value.Count}\n";
            }

            return stats;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Updates tag mappings when an item is updated.
        /// </summary>
        /// <param name="oldItem">Previous version of the item</param>
        /// <param name="newItem">Updated version of the item</param>
        private static void UpdateTagMappings(Item oldItem, Item newItem)
        {
            // Remove old tags
            foreach (string tag in oldItem.Tags)
            {
                if (!newItem.HasTag(tag))
                {
                    _itemsByTag[tag]?.Remove(oldItem.ItemID);
                    if (_itemsByTag[tag]?.Count == 0)
                        _itemsByTag.Remove(tag);
                }
            }

            // Add new tags
            foreach (string tag in newItem.Tags)
            {
                if (!oldItem.HasTag(tag))
                {
                    if (!_itemsByTag.ContainsKey(tag))
                        _itemsByTag[tag] = new List<int>();
                    
                    if (!_itemsByTag[tag].Contains(newItem.ItemID))
                        _itemsByTag[tag].Add(newItem.ItemID);
                }
            }
        }

        #endregion
    }
}
