using UnityEngine;
using Lineage.Ancestral.Legacies.AI;
using Lineage.Ancestral.Legacies.AI.States;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.AI.Examples
{
    /// <summary>
    /// Custom external state machine example for specific Pop behaviors.
    /// Inherits from ExternalStateMachineBase for easier implementation.
    /// </summary>
    public class CustomPopBehavior : ExternalStateMachineBase
    {
        [Header("Custom Behavior Settings")]
        [SerializeField] private float customTriggerChance = 0.05f;
        [SerializeField] private float checkInterval = 2f;
        [SerializeField] private bool enableCustomBehavior = true;
        
        private float timer = 0f;

        public override bool Tick()
        {
            if (!IsActive || !enableCustomBehavior)
                return false;

            // Custom logic timer
            timer += Time.deltaTime;
            if (timer >= checkInterval)
            {
                timer = 0f;

                // Example: Random chance to trigger a custom behavior
                if (Random.value < customTriggerChance)
                {
                    // Force Pop to go idle for a moment (you can change this to any state)
                    ChangeState(new IdleState());
                    Log.AI(pop.name, "Triggered custom behavior");
                    return true; // We handled the tick
                }
            }

            // Use default behavior for everything else
            TickDefault();
            return true;
        }

        public override IState OnStateChangeRequested(IState requestedState)
        {
            // Example: Prevent certain state changes based on conditions
            if (requestedState is ForageState && !enableCustomBehavior)
            {
                Log.AI(pop.name, "Blocked foraging attempt");
                return new IdleState(); // Replace with idle instead
            }

            // Allow the state change
            return requestedState;
        }        protected override void OnAttachedInternal()
        {
            Log.AI(pop.name, "Custom behavior attached");
        }

        protected override void OnDetachedInternal()
        {
            Log.AI(pop?.name ?? "unknown", "Custom behavior detached");
        }
    }
}
