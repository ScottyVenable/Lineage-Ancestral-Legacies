using UnityEngine;
using Lineage.Ancestral.Legacies.Entities;

namespace Lineage.Ancestral.Legacies.AI.States
{
    /// <summary>
    /// State where pops wait/rest without moving around.
    /// </summary>
    public class WaitState : IState
    {
        private Pop pop;
        private float waitTimer;
        private float waitDuration;

        public WaitState(float duration = 5f)
        {
            waitDuration = duration;
        }

        public void Enter(Pop pop)
        {
            this.pop = pop;
            waitTimer = 0f;
            
            // Stop the NavMeshAgent
            var agent = pop.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
            }

            // TODO: Play idle/rest animation
        }

        public void Tick()
        {
            waitTimer += Time.deltaTime;
            
            // Slowly restore rest while waiting
            var needs = pop.GetComponent<Systems.Needs.NeedsComponent>();
            if (needs != null)
            {
                needs.SatisfyRest(10f * Time.deltaTime);
            }

            // Exit wait state after duration
            if (waitTimer >= waitDuration)
            {
                var stateMachine = pop.GetComponent<PopStateMachine>();
                if (stateMachine != null)
                {
                    stateMachine.ChangeState(new IdleState());
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
