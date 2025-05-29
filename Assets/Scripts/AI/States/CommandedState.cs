using UnityEngine;
using UnityEngine.AI;
using Lineage.Ancestral.Legacies.Entities;

namespace Lineage.Ancestral.Legacies.AI.States
{
    public class CommandedState : IState
    {
        private Pop pop;
        private NavMeshAgent agent;
        private Vector3 targetPosition;

        /// <summary>
        /// Create a CommandedState to move pop to a destination.
        /// </summary>
        public CommandedState(Vector3 destination)
        {
            targetPosition = destination;
        }

        public void Enter(Pop pop)
        {
            this.pop = pop;
            agent = pop.GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.SetDestination(targetPosition);
        }

        public void Tick()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Command complete, return to idle
                pop.GetComponent<PopStateMachine>().ChangeState(new IdleState());
            }
        }

        public void Exit()
        {
            // TODO: Cleanup after command
        }
    }
}
