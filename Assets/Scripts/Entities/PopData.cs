using System.Collections.Generic;
using UnityEngine;
using Lineage.Ancestral.Legacies.Systems.TraitSystem;
using Lineage.Ancestral.Legacies.Systems.Inventory;

namespace Lineage.Ancestral.Legacies.Entities
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
        public float maxStamina = 100f;
        public int startingAge = 0;

        public List<TraitSO> startingTraits;
        public List<ItemSO> startingItems;
    }
}
