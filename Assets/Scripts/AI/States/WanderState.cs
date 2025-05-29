using UnityEngine;
using Lineage.Ancestral.Legacies.Entities;

namespace Lineage.Ancestral.Legacies.AI.States
{
    public class WanderState : IState
    {
        private Pop pop;
        private Vector3 targetPosition;
        private float wanderRadius = 2f;

        public void Enter(Pop pop)
        {
            this.pop = pop;
            ChooseNewTarget();
            // TODO: Play walk animation
        }

        public void Tick()
        {
            if (pop.transform.position != targetPosition)
            {
                pop.transform.position = Vector3.MoveTowards(pop.transform.position, targetPosition, Time.deltaTime);
            }
            else
            {
                ChooseNewTarget();
            }
            // TODO: Check for state transitions (e.g., needs)
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
