using Lineage.Ancestral.Legacies.AI.States;
using Lineage.Ancestral.Legacies.Entities;
using UnityEngine;

namespace Lineage.Ancestral.Legacies.AI
{
    /// <summary>
    /// Base implementation of IExternalStateMachine that provides common functionality.
    /// External state machines can inherit from this class to avoid implementing all interface methods.
    /// </summary>
    public abstract class ExternalStateMachineBase : IExternalStateMachine
    {
        protected PopStateMachine popStateMachine;
        protected Pop pop;
        
        [SerializeField] protected bool isActive = true;
        [SerializeField] protected int priority = 0;

        public virtual bool IsActive 
        { 
            get => isActive; 
            protected set => isActive = value; 
        }

        public virtual int Priority 
        { 
            get => priority; 
            set => priority = value; 
        }

        public virtual void OnAttached(PopStateMachine popStateMachine, Pop pop)
        {
            this.popStateMachine = popStateMachine;
            this.pop = pop;
            OnAttachedInternal();
        }

        public virtual void OnDetached()
        {
            OnDetachedInternal();
            this.popStateMachine = null;
            this.pop = null;
        }

        public abstract bool Tick();

        public virtual IState OnStateChangeRequested(IState requestedState)
        {
            // Default behavior: allow the state change
            return requestedState;
        }

        /// <summary>
        /// Called when this external state machine is attached to a PopStateMachine.
        /// Override this in derived classes for custom attachment logic.
        /// </summary>
        protected virtual void OnAttachedInternal() { }

        /// <summary>
        /// Called when this external state machine is detached from a PopStateMachine.
        /// Override this in derived classes for custom detachment logic.
        /// </summary>
        protected virtual void OnDetachedInternal() { }

        /// <summary>
        /// Helper method to change state using the attached PopStateMachine.
        /// </summary>
        protected void ChangeState(IState newState)
        {
            popStateMachine?.ChangeState(newState);
        }

        /// <summary>
        /// Helper method to force a state change using the attached PopStateMachine.
        /// </summary>
        protected void ForceChangeState(IState newState)
        {
            popStateMachine?.ForceChangeState(newState);
        }

        /// <summary>
        /// Helper method to use the default tick behavior.
        /// </summary>
        protected void TickDefault()
        {
            popStateMachine?.TickDefault();
        }

        /// <summary>
        /// Gets the current state from the attached PopStateMachine.
        /// </summary>
        protected IState CurrentState => popStateMachine?.currentState;

        /// <summary>
        /// Sets whether this external state machine is active.
        /// </summary>
        public void SetActive(bool active)
        {
            IsActive = active;
        }
    }
}
