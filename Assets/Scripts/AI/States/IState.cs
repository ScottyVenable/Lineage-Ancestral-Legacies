using Lineage.Ancestral.Legacies.Entities;

namespace Lineage.Ancestral.Legacies.AI.States
{
    /// <summary>
    /// Interface for pop behavior states.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when entering the state.
        /// </summary>
        void Enter(Pop pop);

        /// <summary>
        /// Called each tick to execute state logic.
        /// </summary>
        void Tick();

        /// <summary>
        /// Called when exiting the state.
        /// </summary>
        void Exit();
    }
}
