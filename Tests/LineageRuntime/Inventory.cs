using System.Collections.Generic;

namespace Lineage.Systems.Inventory
{
    /// <summary>
    /// Plain C# inventory implementation used by InventoryComponent and unit tests.
    /// </summary>
    public class Inventory
    {
        public int Capacity { get; set; } = 10;
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();

        public Inventory() { }
        public Inventory(int capacity)
        {
            Capacity = capacity;
        }

        public bool AddItem(string itemId, int count = 1)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0)
                return false;

            if (GetTotalItemCount() + count > Capacity)
                return false;

            if (!_items.ContainsKey(itemId))
                _items[itemId] = 0;

            _items[itemId] += count;
            return true;
        }

        public bool RemoveItem(string itemId, int count = 1)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0)
                return false;

            if (!_items.ContainsKey(itemId) || _items[itemId] < count)
                return false;

            _items[itemId] -= count;
            if (_items[itemId] <= 0)
                _items.Remove(itemId);
            return true;
        }

        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return 0;
            return _items.TryGetValue(itemId, out var count) ? count : 0;
        }

        public Dictionary<string, int> GetAllItems()
        {
            return new Dictionary<string, int>(_items);
        }

        public int GetTotalItemCount()
        {
            int total = 0;
            foreach (var kvp in _items)
                total += kvp.Value;
            return total;
        }

        public bool IsFull() => GetTotalItemCount() >= Capacity;

        public void Clear()
        {
            _items.Clear();
        }
    }
}
