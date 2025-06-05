using System.Collections.Generic;
using UnityEngine;
using Lineage.Ancestral.Legacies.Database;

namespace Lineage.Ancestral.Legacies.Systems.Inventory
{
    /// <summary>
    /// Handles equipping items on an entity using the new GameDatabase structs.
    /// </summary>
    public class EquipmentComponent : MonoBehaviour
    {
        public Dictionary<EquipSlot, int> slotCapacities = new();
        private readonly Dictionary<EquipSlot, List<Item>> _equippedItems = new();

        /// <summary>
        /// Configure how many items can be equipped per slot.
        /// </summary>
        public void ConfigureSlots(IEnumerable<EquipSlotCapacity> capacities)
        {
            slotCapacities.Clear();
            foreach (var cap in capacities)
            {
                slotCapacities[cap.slot] = cap.capacity;
            }
        }

        /// <summary>
        /// Attempt to equip an item. Returns true if successful.
        /// </summary>
        public bool EquipItem(Item item)
        {
            if (!slotCapacities.ContainsKey(item.equipSlot))
                return false;

            if (!_equippedItems.ContainsKey(item.equipSlot))
                _equippedItems[item.equipSlot] = new List<Item>();

            if (_equippedItems[item.equipSlot].Count >= slotCapacities[item.equipSlot])
                return false;

            _equippedItems[item.equipSlot].Add(item);
            return true;
        }

        /// <summary>
        /// Unequip an item from its slot.
        /// </summary>
        public bool UnequipItem(Item item)
        {
            if (_equippedItems.TryGetValue(item.equipSlot, out var list))
            {
                return list.Remove(item);
            }
            return false;
        }

        public IReadOnlyList<Item> GetEquippedItems(EquipSlot slot)
        {
            return _equippedItems.TryGetValue(slot, out var list) ? list : (IReadOnlyList<Item>)System.Array.Empty<Item>();
        }
    }
}
