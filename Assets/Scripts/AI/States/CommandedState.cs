using UnityEngine;
using Lineage.Ancestral.Legacies.Entities;

namespace Lineage.Ancestral.Legacies.AI.States
{
    public class CommandedState : IState
    {
        private Pop pop;
        private Vector3 targetPosition;

        public void Enter(Pop pop)
        {
            this.pop = pop;
            // TODO: Set targetPosition from player command
        }

        public void Tick()
        {
            // TODO: Move towards targetPosition and handle command completion
        }

        public void Exit()
        {
            // TODO: Cleanup after command
        }
    }
}
