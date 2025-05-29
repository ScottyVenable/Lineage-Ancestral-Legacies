using UnityEngine;
using Lineage.Ancestral.Legacies.AI.States;
using Lineage.Ancestral.Legacies.Systems.Needs;

namespace Lineage.Ancestral.Legacies.AI
{
    /// <summary>
    /// Finite State Machine for pop behaviors.
    /// </summary>
    public class PopStateMachine : MonoBehaviour
    {
        private Entities.Pop pop;
        private IState currentState;

        [Header("Auto-Transition Thresholds")]
        public float hungerThreshold = 50f;
        public float thirstThreshold = 50f;
        public float restThreshold = 20f;

        public void Initialize(Entities.Pop popInstance)
        {
            pop = popInstance;
            // Set initial state
            ChangeState(new IdleState());
        }

        public void Tick()
        {
            // Automatic state transitions based on needs
            var needs = pop.GetComponent<NeedsComponent>();
            if (needs.hunger < hungerThreshold && !(currentState is ForageState))
            {
                ChangeState(new ForageState());
                return;
            }
            if (needs.thirst < thirstThreshold && !(currentState is ForageState))
            {
                ChangeState(new ForageState());
                return;
            }
            if (needs.rest < restThreshold && !(currentState is IdleState))
            {
                ChangeState(new IdleState());
                return;
            }

            currentState?.Tick();
        }

        /// <summary>
        /// Transitions to a new state, calling exit and enter methods.
        /// </summary>
        public void ChangeState(IState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter(pop);
        }
    }
}
