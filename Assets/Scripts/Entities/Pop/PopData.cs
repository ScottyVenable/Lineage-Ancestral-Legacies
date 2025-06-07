using System.Collections.Generic;
using UnityEngine;
using Lineage.Systems.TraitSystem;
using Lineage.Systems.Inventory;

namespace Lineage.Entities
{
    /// <summary>
    /// Data container for pop stats, traits, and inventory references.
    /// </summary>
    [CreateAssetMenu(fileName = "PopData", menuName = "Lineage/PopData")]
    public class PopData : ScriptableObject
    {
        public float maxHealth = 100f;
        public float maxHunger = 100f;
        public float maxThirst = 100f;
        public float maxEnergy = 100f; // Changed from maxStamina to maxEnergy for consistency
        public int startingAge = 0;

        public List<TraitSO> startingTraits;
        public List<ItemSO> startingItems;
        
        // Legacy property for backward compatibility
        public float maxStamina => maxEnergy;
    }
}
