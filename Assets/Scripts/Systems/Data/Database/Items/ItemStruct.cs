using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.Database
{
    /// <summary>
    /// Represents an item in the game world with properties for identification, classification, and gameplay mechanics.
    /// Supports stacking, durability, quality degradation, and tag-based functionality.
    /// </summary>
    [Serializable]
    public struct Item
    {
        [Header("Basic Information")]
        [SerializeField] private string _itemName;
        [SerializeField] private int _itemID;
        [SerializeField] private ItemType _itemType;
        [SerializeField] private string _description;

        [Header("Physical Properties")]
        [SerializeField] private float _weight;
        [SerializeField] private int _quantity;
        [SerializeField] private int _maxStackSize;

        [Header("Economic Properties")]
        [SerializeField] private int _baseValue;
        [SerializeField] private int _currentValue;
        [SerializeField] private ItemRarity _itemRarity;

        [Header("Quality & Durability")]
        [SerializeField] private ItemQuality _itemQuality;
        [SerializeField] private float _durability;
        [SerializeField] private float _maxDurability;

        [Header("Gameplay Properties")]
        [SerializeField] private List<string> _tags;
        [SerializeField] private bool _isConsumable;
        [SerializeField] private bool _isQuestItem;
        [SerializeField] private bool _isTradeRestricted;

        [Header("Effects & Bonuses")]
        [SerializeField] private List<StatModifiers> _statModifiers;
        [SerializeField] private string _useEffect;
        [SerializeField] private float _effectPotency;

        // Public Properties
        public string ItemName => _itemName;
        public int ItemID => _itemID;
        public ItemType ItemType => _itemType;
        public string Description => _description;
        public float Weight => _weight;
        public int Quantity => _quantity;
        public int MaxStackSize => _maxStackSize;
        public int BaseValue => _baseValue;
        public int CurrentValue => _currentValue;
        public ItemRarity ItemRarity => _itemRarity;
        public ItemQuality ItemQuality => _itemQuality;
        public float Durability => _durability;
        public float MaxDurability => _maxDurability;
        public List<string> Tags => _tags ?? new List<string>();
        public bool IsConsumable => _isConsumable;
        public bool IsQuestItem => _isQuestItem;
        public bool IsTradeRestricted => _isTradeRestricted;
        public List<StatModifiers> StatModifiers => _statModifiers ?? new List<StatModifiers>();
        public string UseEffect => _useEffect;
        public float EffectPotency => _effectPotency;

        // Computed Properties
        public float TotalWeight => _weight * _quantity;
        public int TotalValue => _currentValue * _quantity;
        public float DurabilityPercentage => _maxDurability > 0 ? (_durability / _maxDurability) * 100f : 100f;
        public bool IsBroken => _itemQuality == ItemQuality.Broken || _itemQuality == ItemQuality.Destroyed;
        public bool CanStack => _maxStackSize > 1;
        public bool IsStackFull => _quantity >= _maxStackSize;
        public bool IsUsable => !IsBroken && (_isConsumable || !string.IsNullOrEmpty(_useEffect));

        /// <summary>
        /// Constructor for creating a new item with basic properties.
        /// </summary>
        public Item(int itemID, string itemName, ItemType itemType, ItemRarity rarity = ItemRarity.Common)
        {
            _itemID = itemID;
            _itemName = itemName ?? string.Empty;
            _itemType = itemType;
            _itemRarity = rarity;
            _description = string.Empty;
            
            _weight = 1f;
            _quantity = 1;
            _maxStackSize = 1;
            
            _baseValue = GetDefaultValue(rarity);
            _currentValue = _baseValue;
            
            _itemQuality = ItemQuality.Good;
            _durability = 100f;
            _maxDurability = 100f;
            
            _tags = new List<string>();
            _isConsumable = itemType == ItemType.Consumable;
            _isQuestItem = itemType == ItemType.QuestItem;
            _isTradeRestricted = false;
            
            _statModifiers = new List<StatModifiers>();
            _useEffect = string.Empty;
            _effectPotency = 1f;
        }

        /// <summary>
        /// Gets the default value based on item rarity.
        /// </summary>
        private static int GetDefaultValue(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Broken => 1,
                ItemRarity.Poor => 5,
                ItemRarity.Common => 25,
                ItemRarity.Uncommon => 100,
                ItemRarity.Rare => 500,
                ItemRarity.Epic => 2500,
                ItemRarity.Legendary => 10000,
                ItemRarity.Artifact => 50000,
                _ => 25
            };
        }

        /// <summary>
        /// Adds a specified quantity to this item stack.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        /// <returns>Amount that couldn't be added due to stack limit</returns>
        public int AddQuantity(int amount)
        {
            if (!CanStack || amount <= 0) return amount;

            int availableSpace = _maxStackSize - _quantity;
            int amountToAdd = Mathf.Min(amount, availableSpace);
            
            _quantity += amountToAdd;
            return amount - amountToAdd;
        }

        /// <summary>
        /// Removes a specified quantity from this item stack.
        /// </summary>
        /// <param name="amount">Amount to remove</param>
        /// <returns>Amount actually removed</returns>
        public int RemoveQuantity(int amount)
        {
            if (amount <= 0) return 0;

            int amountToRemove = Mathf.Min(amount, _quantity);
            _quantity -= amountToRemove;
            return amountToRemove;
        }

        /// <summary>
        /// Damages the item, reducing durability and potentially changing quality.
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        public void TakeDamage(float damage)
        {
            if (_maxDurability <= 0) return;

            _durability = Mathf.Max(0, _durability - damage);
            UpdateQualityFromDurability();
            UpdateValueFromQuality();
        }

        /// <summary>
        /// Repairs the item, restoring durability and potentially improving quality.
        /// </summary>
        /// <param name="repairAmount">Amount to repair</param>
        /// <param name="maxRepairLevel">Maximum quality level this repair can achieve</param>
        public void Repair(float repairAmount, ItemQuality maxRepairLevel = ItemQuality.Good)
        {
            if (_maxDurability <= 0) return;

            _durability = Mathf.Min(_maxDurability, _durability + repairAmount);
            UpdateQualityFromDurability();
            
            // Limit quality to max repair level
            if (_itemQuality > maxRepairLevel)
            {
                _itemQuality = maxRepairLevel;
            }
            
            UpdateValueFromQuality();
        }

        /// <summary>
        /// Updates item quality based on current durability percentage.
        /// </summary>
        private void UpdateQualityFromDurability()
        {
            float durabilityPercent = DurabilityPercentage;
            
            _itemQuality = durabilityPercent switch
            {
                <= 0f => ItemQuality.Destroyed,
                <= 10f => ItemQuality.Broken,
                <= 25f => ItemQuality.Damaged,
                <= 50f => ItemQuality.Worn,
                <= 75f => ItemQuality.Good,
                <= 90f => ItemQuality.Fine,
                <= 99f => ItemQuality.Excellent,
                _ => ItemQuality.Perfect
            };
        }

        /// <summary>
        /// Updates current value based on quality degradation.
        /// </summary>
        private void UpdateValueFromQuality()
        {
            float qualityMultiplier = _itemQuality switch
            {
                ItemQuality.Destroyed => 0f,
                ItemQuality.Broken => 0.1f,
                ItemQuality.Damaged => 0.4f,
                ItemQuality.Worn => 0.7f,
                ItemQuality.Good => 1f,
                ItemQuality.Fine => 1.1f,
                ItemQuality.Excellent => 1.25f,
                ItemQuality.Perfect => 1.5f,
                _ => 1f
            };

            _currentValue = Mathf.RoundToInt(_baseValue * qualityMultiplier);
        }

        /// <summary>
        /// Checks if this item can be combined with another item (for stacking).
        /// </summary>
        /// <param name="other">Other item to check compatibility with</param>
        /// <returns>True if items can be combined</returns>
        public bool CanCombineWith(Item other)
        {
            return _itemID == other._itemID &&
                   _itemType == other._itemType &&
                   _itemRarity == other._itemRarity &&
                   _itemQuality == other._itemQuality &&
                   CanStack && other.CanStack &&
                   !IsStackFull;
        }

        /// <summary>
        /// Adds a tag to this item if it doesn't already exist.
        /// </summary>
        /// <param name="tag">Tag to add</param>
        public void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            
            if (_tags == null) _tags = new List<string>();
            
            if (!_tags.Contains(tag))
            {
                _tags.Add(tag);
            }
        }

        /// <summary>
        /// Removes a tag from this item.
        /// </summary>
        /// <param name="tag">Tag to remove</param>
        /// <returns>True if tag was removed</returns>
        public bool RemoveTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || _tags == null) return false;
            
            return _tags.Remove(tag);
        }

        /// <summary>
        /// Checks if this item has a specific tag.
        /// </summary>
        /// <param name="tag">Tag to check for</param>
        /// <returns>True if item has the tag</returns>
        public bool HasTag(string tag)
        {
            if (string.IsNullOrEmpty(tag) || _tags == null) return false;
            
            return _tags.Contains(tag);
        }

        /// <summary>
        /// Gets the rarity color for UI display.
        /// </summary>
        /// <returns>Color representing the item's rarity</returns>
        public Color GetRarityColor()
        {
            return _itemRarity switch
            {
                ItemRarity.Broken => Color.gray,
                ItemRarity.Poor => new Color(0.6f, 0.6f, 0.6f), // Light gray
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => Color.blue,
                ItemRarity.Epic => new Color(0.5f, 0f, 0.5f), // Purple
                ItemRarity.Legendary => new Color(1f, 0.5f, 0f), // Orange
                ItemRarity.Artifact => Color.red,
                _ => Color.white
            };
        }

        /// <summary>
        /// Creates a detailed string representation of the item.
        /// </summary>
        /// <returns>Formatted item information</returns>
        public override string ToString()
        {
            return $"{_itemName} (ID: {_itemID}) - {_itemType} | " +
                   $"Qty: {_quantity}/{_maxStackSize} | " +
                   $"Quality: {_itemQuality} ({DurabilityPercentage:F1}%) | " +
                   $"Value: {_currentValue} | " +
                   $"Weight: {TotalWeight:F2}";
        }
    }
}
