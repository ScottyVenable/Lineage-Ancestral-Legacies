using UnityEngine;
using Lineage.Ancestral.Legacies.AI.States;
using Lineage.Ancestral.Legacies.Systems.Needs;
using System.Collections.Generic;
using System.Linq;

namespace Lineage.Ancestral.Legacies.AI
{
    /// <summary>
    /// Finite State Machine for pop behaviors.
    /// Supports attaching external state machines that can override default behavior.
    /// </summary>
    public class PopStateMachine : MonoBehaviour    {
        private Entities.Pop pop;
        public IState currentState; // Made public for save/load functionality

        [Header("Auto-Transition Thresholds")]
        public float hungerThreshold = 50f;
        public float thirstThreshold = 50f;
        public float restThreshold = 20f;

        [Header("External State Machine")]
        [SerializeField] private bool allowExternalStateMachines = true;
        
        // List of attached external state machines, sorted by priority
        private List<IExternalStateMachine> externalStateMachines = new List<IExternalStateMachine>();        public void Initialize(Entities.Pop popInstance)
        {
            pop = popInstance;
            // Set initial state
            ChangeState(new IdleState());
        }

        /// <summary>
        /// Attaches an external state machine to this PopStateMachine.
        /// </summary>
        /// <param name="externalStateMachine">The external state machine to attach</param>
        public void AttachExternalStateMachine(IExternalStateMachine externalStateMachine)
        {
            if (!allowExternalStateMachines || externalStateMachine == null)
                return;

            // Remove if already attached
            DetachExternalStateMachine(externalStateMachine);

            // Add and sort by priority (highest first)
            externalStateMachines.Add(externalStateMachine);
            externalStateMachines = externalStateMachines.OrderByDescending(esm => esm.Priority).ToList();

            // Notify the external state machine it was attached
            externalStateMachine.OnAttached(this, pop);
        }

        /// <summary>
        /// Detaches an external state machine from this PopStateMachine.
        /// </summary>
        /// <param name="externalStateMachine">The external state machine to detach</param>
        public void DetachExternalStateMachine(IExternalStateMachine externalStateMachine)
        {
            if (externalStateMachines.Remove(externalStateMachine))
            {
                externalStateMachine.OnDetached();
            }
        }

        /// <summary>
        /// Detaches all external state machines.
        /// </summary>
        public void DetachAllExternalStateMachines()
        {
            foreach (var esm in externalStateMachines.ToList())
            {
                DetachExternalStateMachine(esm);
            }
        }

        /// <summary>
        /// Gets the currently attached external state machines.
        /// </summary>
        public IReadOnlyList<IExternalStateMachine> GetExternalStateMachines()
        {
            return externalStateMachines.AsReadOnly();
        }

        /// <summary>
        /// Gets whether external state machines are allowed on this PopStateMachine.
        /// </summary>
        public bool AllowExternalStateMachines
        {
            get => allowExternalStateMachines;
            set => allowExternalStateMachines = value;
        }        public void Tick()
        {
            // Let external state machines handle ticking if any are active
            if (allowExternalStateMachines && externalStateMachines.Any())
            {
                foreach (var externalStateMachine in externalStateMachines)
                {
                    if (externalStateMachine.IsActive && externalStateMachine.Tick())
                    {
                        // External state machine handled the tick, skip default behavior
                        return;
                    }
                }
            }

            // Use default behavior
            TickDefault();
        }

        /// <summary>
        /// Default tick behavior (original implementation).
        /// Can be called by external state machines if they want to use default logic.
        /// </summary>
        public void TickDefault()
        {
            // Only auto-transition if not in commanded state (let player commands complete)
            if (currentState is CommandedState)
            {
                currentState?.Tick();
                return;
            }

            // Automatic state transitions based on needs
            var needs = pop.GetComponent<NeedsComponent>();
            if (needs != null && needs.hunger < hungerThreshold && !(currentState is ForageState))
            {
                ChangeState(new ForageState());
                return;
            }
            if (needs != null && needs.thirst < thirstThreshold && !(currentState is ForageState))
            {
                ChangeState(new ForageState());
                return;
            }
            if (needs != null && needs.rest < restThreshold && !(currentState is IdleState))
            {
                ChangeState(new IdleState()); // Rest while idle
                return;
            }

            currentState?.Tick();
        }        /// <summary>
        /// Transitions to a new state, calling exit and enter methods.
        /// External state machines can override the requested state change.
        /// </summary>
        public void ChangeState(IState newState)
        {
            // Allow external state machines to override the state change
            IState actualState = newState;
            if (allowExternalStateMachines && externalStateMachines.Any())
            {
                foreach (var externalStateMachine in externalStateMachines)
                {
                    if (externalStateMachine.IsActive)
                    {
                        var overriddenState = externalStateMachine.OnStateChangeRequested(actualState);
                        if (overriddenState != null)
                        {
                            actualState = overriddenState;
                            break; // First active external state machine wins
                        }
                        else if (overriddenState == null && actualState == newState)
                        {
                            // External state machine prevented the change
                            return;
                        }
                    }
                }
            }

            // Perform the state change
            currentState?.Exit();
            currentState = actualState;
            currentState?.Enter(pop);
        }

        /// <summary>
        /// Forces a state change without external state machine intervention.
        /// Use this for critical state changes that should not be overridden.
        /// </summary>
        public void ForceChangeState(IState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter(pop);
        }

        private void OnDestroy()
        {
            // Clean up external state machines when destroyed
            DetachAllExternalStateMachines();
        }
    }
}
