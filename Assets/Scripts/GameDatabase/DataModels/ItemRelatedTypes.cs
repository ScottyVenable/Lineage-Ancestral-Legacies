using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    #region Item System    /// <summary>
    /// Represents an item in the game, including its type, rarity, quality, and other properties.
    /// </summary>
    public struct Item
    {
        public enum ID
        {
            None = 0,
            Sword = 1,
            Shield = 2,
            Bow = 3,
            Arrow = 4,
            Potion = 5,
            Bread = 6,
            Water = 7,
            Wood = 8,
            Stone = 9,
            Iron = 10,
            Gold = 11,
            Gem = 12,
            Herb = 13,
            Meat = 14,
            Hide = 15,
            Cloth = 16,
            Rope = 17,
            Tool = 18,
            Key = 19,
            Book = 20,
            Scroll = 21,
            Ring = 22,
            Amulet = 23,
            Armor = 24,
            Helmet = 25,
            Boots = 26,
            Gloves = 27,
            // Additional items for the game
            IronSword = 28,
            SteelAxe = 29,
            EnchantedStaff = 30,
            LeatherArmor = 31,
            ChainMail = 32,
            DragonScaleArmor = 33,
            HealthPotion = 34,
            ManaPotion = 35
        }        // Legacy enum compatibility properties - return default values for backwards compatibility
        public ItemType LegacyItemType => itemType;
        public ItemRarity LegacyItemRarity => itemRarity;
        public ItemQuality LegacyItemQuality => itemQuality;
        public EquipSlot LegacyItemSlot => equipSlot;

        // Core data fields
        public ID itemID;
        public string itemName;
        public string itemDescription;
        public ItemType itemType;

        // Equipment information
        public EquipSlot equipSlot;

        // Additional properties
        public float weight;
        public int quantity;
        public int value;
        public ItemRarity itemRarity;
        public ItemQuality itemQuality;
        public List<string> tags;

        // Constructors
        public Item(string name, ID id, ItemType type, float weight = 1f, int quantity = 1, int value = 10, 
                   ItemRarity rarity = ItemRarity.Common, ItemQuality quality = ItemQuality.Normal)
        {
            itemName = name;
            itemID = id;
            itemDescription = "";
            itemType = type;
            this.weight = weight;
            this.quantity = quantity;
            this.value = value;
            itemRarity = rarity;
            itemQuality = quality;
            tags = new List<string>();
            
            // Set default equipment slot based on item type
            equipSlot = type switch
            {
                ItemType.Weapon => EquipSlot.Weapon,
                ItemType.Armor => EquipSlot.Chest,
                ItemType.Accessory => EquipSlot.Accessory,
                _ => EquipSlot.Accessory
            };
        }

        public Item(string name, int id, ItemType type, float weight = 1f, int quantity = 1, int value = 10, 
                   ItemRarity rarity = ItemRarity.Common, ItemQuality quality = ItemQuality.Normal)
        {
            itemName = name;
            itemID = (ID)id;
            itemDescription = "";
            itemType = type;
            this.weight = weight;
            this.quantity = quantity;
            this.value = value;
            itemRarity = rarity;
            itemQuality = quality;
            tags = new List<string>();
              // Set default equipment slot based on item type
            equipSlot = type switch
            {
                ItemType.Weapon => EquipSlot.Weapon,
                ItemType.Armor => EquipSlot.Chest,
                ItemType.Accessory => EquipSlot.Accessory,
                _ => EquipSlot.Accessory
            };
        }
    }

    #endregion
}
