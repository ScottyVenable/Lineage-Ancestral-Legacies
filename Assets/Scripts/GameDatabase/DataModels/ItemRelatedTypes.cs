using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Database
{
    #region Item System

    /// <summary>
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
            Gloves = 27
        }

        public string itemName;
        public int itemID;
        public ItemType itemType;
        public 
        public float weight;
        public int quantity;
        public int value;
        public ItemRarity itemRarity;
        public ItemQuality itemQuality;
        public List<string> tags;
    }

    #endregion
}
