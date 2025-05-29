using UnityEngine;

namespace Lineage.Ancestral.Legacies.Systems.Inventory
{
    /// <summary>
    /// ScriptableObject representing an inventory item.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Lineage/Inventory/Item")]
    public class ItemSO : ScriptableObject
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
        public int maxStack = 99;
    }
}
