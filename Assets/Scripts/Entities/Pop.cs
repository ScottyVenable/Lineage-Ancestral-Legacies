using UnityEngine;

namespace Lineage.Ancestral.Legacies.Entities
{
    /// <summary>
    /// Represents an individual pop with core attributes and components.
    /// </summary>
    public class Pop : MonoBehaviour
    {
        [Header("Core Attributes")]
        public float health = 100f;
        public float hunger = 100f;
        public float thirst = 100f;
        public float stamina = 100f;
        public int age;
        public bool isAdult;

        // References to system components
        private Systems.Needs.NeedsComponent needsComponent;
        private Systems.Inventory.InventoryComponent inventoryComponent;
        private AI.PopStateMachine stateMachine;

        private void Awake()
        {
            needsComponent = GetComponent<Systems.Needs.NeedsComponent>();
            inventoryComponent = GetComponent<Systems.Inventory.InventoryComponent>();
            stateMachine = GetComponent<AI.PopStateMachine>();
        }

        private void Start()
        {
            stateMachine.Initialize(this);
        }

        private void Update()
        {
            needsComponent.UpdateNeeds(Time.deltaTime);
            stateMachine.Tick();
        }
    }
}
