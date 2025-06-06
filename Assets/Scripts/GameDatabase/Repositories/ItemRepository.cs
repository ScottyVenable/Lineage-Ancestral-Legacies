using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Ancestral.Legacies.Debug;

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
        /// </summary>        /// <summary>
        /// Gets an item by its ID.
        /// </summary>
        /// <param name="itemID">The ID of the item to retrieve.</param>
        /// <returns>The item with the specified ID, or default if not found.</returns>
        public static Item GetItemByID(int itemID)
        {
            return AllItems.FirstOrDefault(i => (int)i.itemID == itemID);
        }

        /// <summary>
        /// Gets an item by its ID.
        /// </summary>
        /// <param name="itemID">The ID of the item to retrieve.</param>
        /// <returns>The item with the specified ID, or default if not found.</returns>
        public static Item GetItemByID(Item.ID itemID)
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

        #region Initialization        /// <summary>
        /// Initializes the item database with default data.
        /// </summary>
        public static void InitializeDatabase()
        {
            AllItems.Clear();
            
            // Weapons
            var ironSword = new Item("Iron Sword", Item.ID.IronSword, ItemType.Weapon, 3f, 1, 50, ItemRarity.Common, ItemQuality.Good);
            ironSword.tags = new List<string> { "Weapon", "Melee", "Sword" };
            ironSword.equipSlot = EquipSlot.Weapon;
            AllItems.Add(ironSword);

            var steelAxe = new Item("Steel Axe", Item.ID.SteelAxe, ItemType.Weapon, 4f, 1, 75, ItemRarity.Common, ItemQuality.Good);
            steelAxe.tags = new List<string> { "Weapon", "Melee", "Axe" };
            steelAxe.equipSlot = EquipSlot.Weapon;
            AllItems.Add(steelAxe);

            var enchantedStaff = new Item("Enchanted Staff", Item.ID.EnchantedStaff, ItemType.Weapon, 2f, 1, 150, ItemRarity.Rare, ItemQuality.Excellent);
            enchantedStaff.tags = new List<string> { "Weapon", "Magic", "Staff" };
            enchantedStaff.equipSlot = EquipSlot.Weapon;
            AllItems.Add(enchantedStaff);

            // Armor
            var leatherArmor = new Item("Leather Armor", Item.ID.LeatherArmor, ItemType.Armor, 5f, 1, 30, ItemRarity.Common, ItemQuality.Fair);
            leatherArmor.tags = new List<string> { "Armor", "Light", "Chest" };
            leatherArmor.equipSlot = EquipSlot.Chest;
            AllItems.Add(leatherArmor);

            var chainMail = new Item("Chain Mail", Item.ID.ChainMail, ItemType.Armor, 8f, 1, 80, ItemRarity.Uncommon, ItemQuality.Good);
            chainMail.tags = new List<string> { "Armor", "Medium", "Chest" };
            chainMail.equipSlot = EquipSlot.Chest;
            AllItems.Add(chainMail);

            var dragonScaleArmor = new Item("Dragon Scale Armor", Item.ID.DragonScaleArmor, ItemType.Armor, 15f, 1, 500, ItemRarity.Legendary, ItemQuality.Masterwork);
            dragonScaleArmor.tags = new List<string> { "Armor", "Heavy", "Chest", "Dragon" };
            dragonScaleArmor.equipSlot = EquipSlot.Chest;
            AllItems.Add(dragonScaleArmor);

            // Consumables
            var healthPotion = new Item("Health Potion", Item.ID.HealthPotion, ItemType.Consumable, 0.5f, 5, 25, ItemRarity.Common, ItemQuality.Good);
            healthPotion.tags = new List<string> { "Consumable", "Potion", "Healing" };
            AllItems.Add(healthPotion);

            var manaPotion = new Item("Mana Potion", Item.ID.ManaPotion, ItemType.Consumable, 0.5f, 3, 35, ItemRarity.Common, ItemQuality.Good);
            manaPotion.tags = new List<string> { "Consumable", "Potion", "Mana" };
            AllItems.Add(manaPotion);

            var bread = new Item("Bread", Item.ID.Bread, ItemType.Consumable, 0.2f, 10, 5, ItemRarity.Common, ItemQuality.Fair);
            bread.tags = new List<string> { "Consumable", "Food" };
            AllItems.Add(bread);

            // Materials
            var wood = new Item("Wood", Item.ID.Wood, ItemType.Material, 0.5f, 20, 2, ItemRarity.Common, ItemQuality.Normal);
            wood.tags = new List<string> { "Material", "Crafting", "Wood" };
            AllItems.Add(wood);

            var stone = new Item("Stone", Item.ID.Stone, ItemType.Material, 1f, 15, 3, ItemRarity.Common, ItemQuality.Normal);
            stone.tags = new List<string> { "Material", "Crafting", "Stone" };
            AllItems.Add(stone);

            var iron = new Item("Iron Ore", Item.ID.Iron, ItemType.Material, 2f, 10, 8, ItemRarity.Common, ItemQuality.Normal);
            iron.tags = new List<string> { "Material", "Crafting", "Metal", "Ore" };
            AllItems.Add(iron);

            Log.Info("ItemRepository: Database initialized successfully.", Log.LogCategory.Systems);
        }
        #endregion
    }
}
