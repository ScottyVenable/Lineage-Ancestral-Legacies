using UnityEngine;
using UnityEngine.AI;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Debug;

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
            
            // Ensure the agent is on the NavMesh
            PlaceAgentOnNavMesh();
            
            ChooseNewTarget();
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(targetPosition);
            }
            // TODO: Trigger walk animation via pop Animator
        }

        public void Tick()
        {
            // If not on NavMesh, try to place on NavMesh
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            {
                PlaceAgentOnNavMesh();
                return;
            }

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

        private void PlaceAgentOnNavMesh()
        {
            if (agent == null) return;
            
            // Try to find the nearest point on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pop.transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                // Move the agent to the NavMesh
                pop.transform.position = hit.position;
                
                // After moving, we need to ensure the agent is on the NavMesh
                if (!agent.isOnNavMesh)
                {
                    // If still not on NavMesh, we might need to manually enable it
                    agent.enabled = false;
                    agent.enabled = true;
                }
            }
            else
            {
                Debug.Log.Warning("Cannot find nearby NavMesh position for agent: " + pop.name);
            }
        }
    }
}
