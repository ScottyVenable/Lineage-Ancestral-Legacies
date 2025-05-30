using UnityEngine;
using Lineage.Ancestral.Legacies.Entities;

namespace Lineage.Ancestral.Legacies.AI.States
{
    public class IdleState : IState
    {
        private Pop pop;
        private float idleTimer;
        private float idleDuration;

        public void Enter(Pop pop)
        {
            this.pop = pop;
            idleTimer = 0f;
            idleDuration = Random.Range(2f, 8f); // Random idle time
            
            // Stop the NavMeshAgent
            var agent = pop.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
            }
            
            // TODO: Play idle animation
        }

        public void Tick()
        {
            idleTimer += Time.deltaTime;
            
            // After idle duration, transition to wandering
            if (idleTimer >= idleDuration)
            {
                var stateMachine = pop.GetComponent<PopStateMachine>();
                if (stateMachine != null)
                {
                    stateMachine.ChangeState(new WanderState());
                }
            }
        }

        public void Exit()
        {
            // Resume NavMeshAgent movement
            var agent = pop.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
            else if (agent != null)
            {
                // Try to place agent on NavMesh before resuming
                PlaceAgentOnNavMesh(agent);
            }
        }
        
        private void PlaceAgentOnNavMesh(UnityEngine.AI.NavMeshAgent agent)
        {
            if (agent == null || pop == null) return;
            
            // Try to find the nearest point on the NavMesh
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(pop.transform.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // Move the agent to the NavMesh
                pop.transform.position = hit.position;
                
                // After moving, we need to ensure the agent is on the NavMesh
                if (!agent.isOnNavMesh)
                {
                    // If still not on NavMesh, we might need to manually enable it
                    agent.enabled = false;
                    agent.enabled = true;
                    
                    // Now try to set isStopped if the agent is on NavMesh
                    if (agent.isOnNavMesh)
                    {
                        agent.isStopped = false;
                    }
                }
            }
            else
            {
                Debug.Log.Warning("Cannot find nearby NavMesh position for agent: " + pop.name);
            }
        }
    }
}
