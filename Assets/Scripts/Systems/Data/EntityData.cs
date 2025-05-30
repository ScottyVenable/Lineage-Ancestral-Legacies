using UnityEngine;
using Lineage.Ancestral.Legacies.Systems.Needs;
using Lineage.Ancestral.Legacies.Systems.Inventory;
using Lineage.Ancestral.Legacies.AI;
using Lineage.Ancestral.Legacies.Managers;
using Lineage.Ancestral.Legacies.Debug;
using UnityEngine.UI;
using UnityEngine.AI;

namespace Lineage.Ancestral.Legacies.EntityData
{
    /// <summary>
    /// Represents data for an entity in the game.
    /// </summary>
    public class EntityData : MonoBehaviour
    {
        [Header("Entity Data")]
        public string entityName;
        public entityID;
        public float health = 100f;
        public float maxHealth = 100f;
        public float mana = 100f;
        public float maxMana = 100f;
        public List<string> tags = new List<string>();

        [Header("Components")]
        public NeedsComponent needsComponent;
        public InventoryComponent inventoryComponent;
        public AIController aiController;

        [Header("Debugging")]
        public DebugManager debugManager;
    }
}