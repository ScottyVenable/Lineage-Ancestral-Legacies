using UnityEngine;
using UnityEngine.AI;
using Lineage.Ancestral.Legacies.Entities;

namespace Lineage.Ancestral.Legacies.AI.States
{
    public class WanderState : IState
    {
        private Pop pop;
        private NavMeshAgent agent;
        private Vector3 targetPosition;
        private float wanderRadius = 2f;

        public void Enter(Pop pop)
        {
            this.pop = pop;
            agent = pop.GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            ChooseNewTarget();
            agent.SetDestination(targetPosition);
            // TODO: Trigger walk animation via pop Animator
        }

        public void Tick()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                ChooseNewTarget();
                agent.SetDestination(targetPosition);
            }
        }

        public void Exit()
        {
            // TODO: Cleanup
        }

        private void ChooseNewTarget()
        {
            Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
            targetPosition = pop.transform.position + new Vector3(randomPoint.x, randomPoint.y, 0);
        }
    }
}
