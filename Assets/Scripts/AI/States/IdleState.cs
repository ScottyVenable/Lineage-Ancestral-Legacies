using UnityEngine;
using Lineage.Ancestral.Legacies.Entities;

namespace Lineage.Ancestral.Legacies.AI.States
{
    public class IdleState : IState
    {
        private Pop pop;

        public void Enter(Pop pop)
        {
            this.pop = pop;
            // TODO: Play idle animation
        }

        public void Tick()
        {
            // TODO: Transition to Wander or SatisfyNeed based on conditions
        }

        public void Exit()
        {
            // TODO: Cleanup
        }
    }
}
