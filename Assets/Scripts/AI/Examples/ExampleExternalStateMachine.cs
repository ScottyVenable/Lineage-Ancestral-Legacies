using Lineage.Ancestral.Legacies.AI.States;
using Lineage.Ancestral.Legacies.Entities;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.AI.Examples
{
    /// <summary>
    /// Example external state machine that demonstrates how to create custom Pop behaviors.
    /// This example makes Pops wander more frequently and prevents them from foraging.
    /// </summary>
    public class ExampleExternalStateMachine : MonoBehaviour, IExternalStateMachine
    {
        [Header("Example Settings")]
        [SerializeField] private float wanderChance = 0.1f;
        [SerializeField] private bool preventForaging = true;
        [SerializeField] private float wanderTimer = 0f;
        [SerializeField] private float wanderCheckInterval = 1f;
        [SerializeField] private bool isActive = true;
        [SerializeField] private int priority = 0;

        private PopStateMachine popStateMachine;
        private Pop pop;

        public bool IsActive 
        { 
            get => isActive; 
            private set => isActive = value; 
        }

        public int Priority 
        { 
            get => priority; 
            set => priority = value; 
        }

        public void OnAttached(PopStateMachine popStateMachine, Pop pop)
        {
            this.popStateMachine = popStateMachine;
            this.pop = pop;
            UnityEngine.Debug.Log($"[ExampleExternalStateMachine] Attached to {pop.name}");
        }

        public void OnDetached()
        {
            UnityEngine.Debug.Log($"[ExampleExternalStateMachine] Detached from {pop?.name ?? "unknown pop"}");
            this.popStateMachine = null;
            this.pop = null;
        }

        public bool Tick()
        {
            if (!IsActive)
                return false;

            // Check if we should trigger wandering
            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderCheckInterval)
            {
                wanderTimer = 0f;
                
                // Random chance to start wandering if idle
                if (popStateMachine.currentState is IdleState && Random.value < wanderChance)
                {
                    popStateMachine.ChangeState(new WanderState());
                    return true; // We handled the tick
                }
            }

            // Let default behavior handle the rest
            popStateMachine.TickDefault();
            return true; // We still handled the tick (even though we delegated)
        }

        public IState OnStateChangeRequested(IState requestedState)
        {
            if (!IsActive)
                return requestedState;

            // Prevent foraging if the setting is enabled
            if (preventForaging && requestedState is ForageState)
            {
                UnityEngine.Debug.Log($"[ExampleExternalStateMachine] Preventing {pop.name} from foraging, switching to wander instead.");
                return new WanderState();
            }

            // Allow other state changes
            return requestedState;
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }

        #region Inspector Methods (for testing in editor)
        
        [Header("Debug Controls")]
        [SerializeField] private bool debugMode = false;

        [ContextMenu("Force Wander")]
        private void ForceWander()
        {
            if (debugMode && IsActive && popStateMachine != null)
            {
                popStateMachine.ChangeState(new WanderState());
            }
        }

        [ContextMenu("Toggle Active")]
        private void ToggleActive()
        {
            SetActive(!IsActive);
            UnityEngine.Debug.Log($"[ExampleExternalStateMachine] Active: {IsActive}");
        }

        #endregion
    }
}
