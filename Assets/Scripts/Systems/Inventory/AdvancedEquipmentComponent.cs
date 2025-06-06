using System.Collections.Generic;
using UnityEngine;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Ancestral.Legacies.Systems.Inventory
{
    /// <summary>
    /// Manages equipment slot configurations for different entity types.
    /// Handles special cases like animals without certain slots (e.g., wolves can't wear rings).
    /// </summary>
    [System.Serializable]
    public class EntityEquipmentProfile
    {
        [Header("Entity Type Configuration")]
        public string profileName = "Default";
        public EntityType entityType = EntityType.Humanoid;
        
        [Header("Equipment Slot Capacities")]
        public List<EquipSlotCapacity> allowedSlots = new List<EquipSlotCapacity>();
        
        /// <summary>
        /// Default equipment profiles for common entity types.
        /// </summary>
        public static Dictionary<EntityType, EntityEquipmentProfile> DefaultProfiles = new Dictionary<EntityType, EntityEquipmentProfile>
        {
            {
                EntityType.Humanoid, new EntityEquipmentProfile
                {
                    profileName = "Humanoid",
                    entityType = EntityType.Humanoid,
                    allowedSlots = new List<EquipSlotCapacity>
                    {
                        new EquipSlotCapacity(EquipSlot.Head, 1),
                        new EquipSlotCapacity(EquipSlot.Chest, 1),
                        new EquipSlotCapacity(EquipSlot.Legs, 1),
                        new EquipSlotCapacity(EquipSlot.Feet, 1),
                        new EquipSlotCapacity(EquipSlot.Hands, 1),
                        new EquipSlotCapacity(EquipSlot.Weapon, 2), // Main hand + off hand
                        new EquipSlotCapacity(EquipSlot.Ring, 2),   // Two rings
                        new EquipSlotCapacity(EquipSlot.Neck, 1),
                        new EquipSlotCapacity(EquipSlot.Accessory, 3) // Multiple accessories
                    }
                }
            },
            {
                EntityType.Animal, new EntityEquipmentProfile
                {
                    profileName = "Animal",
                    entityType = EntityType.Animal,
                    allowedSlots = new List<EquipSlotCapacity>
                    {
                        new EquipSlotCapacity(EquipSlot.Neck, 1),     // Can wear collars/necklaces
                        new EquipSlotCapacity(EquipSlot.Accessory, 1) // Basic accessories only
                    }
                }
            },
            {
                EntityType.Construct, new EntityEquipmentProfile
                {
                    profileName = "Construct",
                    entityType = EntityType.Construct,
                    allowedSlots = new List<EquipSlotCapacity>
                    {
                        new EquipSlotCapacity(EquipSlot.Weapon, 1),
                        new EquipSlotCapacity(EquipSlot.Accessory, 2) // Mechanical accessories
                    }
                }
            },
            {
                EntityType.Undead, new EntityEquipmentProfile
                {
                    profileName = "Undead",
                    entityType = EntityType.Undead,
                    allowedSlots = new List<EquipSlotCapacity>
                    {
                        new EquipSlotCapacity(EquipSlot.Head, 1),
                        new EquipSlotCapacity(EquipSlot.Chest, 1),
                        new EquipSlotCapacity(EquipSlot.Weapon, 1),
                        new EquipSlotCapacity(EquipSlot.Accessory, 1) // Limited equipment
                    }
                }
            }
        };
        
        /// <summary>
        /// Check if this profile allows equipping items in the specified slot.
        /// </summary>
        public bool CanEquipInSlot(EquipSlot slot)
        {
            return allowedSlots.Exists(s => s.slot == slot);
        }
        
        /// <summary>
        /// Get the maximum capacity for a specific slot.
        /// </summary>
        public int GetSlotCapacity(EquipSlot slot)
        {
            var slotCapacity = allowedSlots.Find(s => s.slot == slot);
            return slotCapacity.capacity;
        }
    }
    
    /// <summary>
    /// Enhanced equipment component with entity-specific slot management.
    /// </summary>
    public class AdvancedEquipmentComponent : MonoBehaviour
    {
        [Header("Equipment Configuration")]
        [SerializeField] private EntityEquipmentProfile equipmentProfile;
        [SerializeField] private EntityType entityType = EntityType.Humanoid;
        [SerializeField] private bool useDefaultProfile = true;
        
        [Header("Current Equipment")]
        [SerializeField] private List<EquippedItem> currentEquipment = new List<EquippedItem>();
        
        private readonly Dictionary<EquipSlot, List<Item>> _equippedItems = new Dictionary<EquipSlot, List<Item>>();
        
        [System.Serializable]
        public struct EquippedItem
        {
            public EquipSlot slot;
            public Item item;
            public bool isActive;
        }
        
        private void Awake()
        {
            InitializeEquipmentProfile();
        }
        
        /// <summary>
        /// Initialize the equipment profile based on entity type.
        /// </summary>
        private void InitializeEquipmentProfile()
        {
            if (useDefaultProfile && EntityEquipmentProfile.DefaultProfiles.TryGetValue(entityType, out var defaultProfile))
            {
                equipmentProfile = defaultProfile;
            }
            
            if (equipmentProfile != null)
            {
                InitializeSlots();
            }
        }
        
        /// <summary>
        /// Initialize available equipment slots.
        /// </summary>
        private void InitializeSlots()
        {
            _equippedItems.Clear();
            foreach (var slotCapacity in equipmentProfile.allowedSlots)
            {
                _equippedItems[slotCapacity.slot] = new List<Item>();
            }
        }
        
        /// <summary>
        /// Attempt to equip an item. Returns true if successful.
        /// </summary>
        public bool EquipItem(Item item)
        {
            // Check if the entity can equip items in this slot
            if (!equipmentProfile.CanEquipInSlot(item.equipSlot))
            {
                Debug.LogWarning($"{entityType} entities cannot equip items in slot: {item.equipSlot}");
                return false;
            }
            
            // Check if slot has capacity
            if (!_equippedItems.ContainsKey(item.equipSlot))
                _equippedItems[item.equipSlot] = new List<Item>();
                
            var maxCapacity = equipmentProfile.GetSlotCapacity(item.equipSlot);
            if (_equippedItems[item.equipSlot].Count >= maxCapacity)
            {
                Debug.LogWarning($"Slot {item.equipSlot} is at maximum capacity ({maxCapacity})");
                return false;
            }
            
            // Equip the item
            _equippedItems[item.equipSlot].Add(item);
            currentEquipment.Add(new EquippedItem 
            { 
                slot = item.equipSlot, 
                item = item, 
                isActive = true 
            });
            
            Debug.Log($"Equipped {item.itemName} in slot {item.equipSlot}");
            return true;
        }
        
        /// <summary>
        /// Unequip an item from its slot.
        /// </summary>
        public bool UnequipItem(Item item)
        {
            if (_equippedItems.TryGetValue(item.equipSlot, out var list))
            {
                bool removed = list.Remove(item);
                if (removed)
                {
                    currentEquipment.RemoveAll(e => e.item.itemID == item.itemID);
                    Debug.Log($"Unequipped {item.itemName} from slot {item.equipSlot}");
                }
                return removed;
            }
            return false;
        }
        
        /// <summary>
        /// Get all items equipped in a specific slot.
        /// </summary>
        public IReadOnlyList<Item> GetEquippedItems(EquipSlot slot)
        {
            return _equippedItems.TryGetValue(slot, out var list) ? list : (IReadOnlyList<Item>)System.Array.Empty<Item>();
        }
        
        /// <summary>
        /// Get all currently equipped items.
        /// </summary>
        public List<Item> GetAllEquippedItems()
        {
            var allItems = new List<Item>();
            foreach (var itemList in _equippedItems.Values)
            {
                allItems.AddRange(itemList);
            }
            return allItems;
        }
        
        /// <summary>
        /// Check if an item can be equipped based on slot restrictions.
        /// </summary>
        public bool CanEquip(Item item)
        {
            return equipmentProfile.CanEquipInSlot(item.equipSlot) && 
                   GetEquippedItems(item.equipSlot).Count < equipmentProfile.GetSlotCapacity(item.equipSlot);
        }
        
        /// <summary>
        /// Get available equipment slots for this entity.
        /// </summary>
        public List<EquipSlot> GetAvailableSlots()
        {
            var slots = new List<EquipSlot>();
            foreach (var capacity in equipmentProfile.allowedSlots)
            {
                slots.Add(capacity.slot);
            }
            return slots;
        }
        
        /// <summary>
        /// Get slot capacity information.
        /// </summary>
        public Dictionary<EquipSlot, int> GetSlotCapacities()
        {
            var capacities = new Dictionary<EquipSlot, int>();
            foreach (var capacity in equipmentProfile.allowedSlots)
            {
                capacities[capacity.slot] = capacity.capacity;
            }
            return capacities;
        }
        
        /// <summary>
        /// Update the entity type and reinitialize equipment profile.
        /// </summary>
        public void SetEntityType(EntityType newEntityType)
        {
            entityType = newEntityType;
            InitializeEquipmentProfile();
        }
    }
}
