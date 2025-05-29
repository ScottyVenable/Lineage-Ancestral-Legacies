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
            if (agent != null)
            {
                agent.isStopped = false;
            }
        }
    }
}
