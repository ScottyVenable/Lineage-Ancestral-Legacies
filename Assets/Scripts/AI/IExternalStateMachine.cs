using Lineage.Ancestral.Legacies.AI.States;
using Lineage.Ancestral.Legacies.Entities;

namespace Lineage.Ancestral.Legacies.AI
{
    /// <summary>
    /// Interface for external state machines that can be attached to PopStateMachine.
    /// External state machines can override the default behavior and control state transitions.
    /// </summary>
    public interface IExternalStateMachine
    {
        /// <summary>
        /// Called when the external state machine is attached to a PopStateMachine.
        /// </summary>
        /// <param name="popStateMachine">The PopStateMachine this external state machine is attached to</param>
        /// <param name="pop">The Pop entity being controlled</param>
        void OnAttached(PopStateMachine popStateMachine, Pop pop);

        /// <summary>
        /// Called when the external state machine is detached from a PopStateMachine.
        /// </summary>
        void OnDetached();

        /// <summary>
        /// Called each tick to allow the external state machine to control behavior.
        /// </summary>
        /// <returns>True if the external state machine handled the tick, false to use default behavior</returns>
        bool Tick();

        /// <summary>
        /// Called to allow the external state machine to override state changes.
        /// </summary>
        /// <param name="requestedState">The state that was requested to change to</param>
        /// <returns>The actual state to change to, or null to prevent the change</returns>
        IState OnStateChangeRequested(IState requestedState);

        /// <summary>
        /// Gets whether this external state machine is currently active/enabled.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets or sets the priority of this external state machine when multiple are attached.
        /// Higher values have higher priority.
        /// </summary>
        int Priority { get; set; }
    }
}
