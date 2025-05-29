using UnityEngine;
using UnityEngine.AI;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Systems.Inventory;
using Lineage.Ancestral.Legacies.Systems.Resource;
using Lineage.Ancestral.Legacies.Systems.Needs;

namespace Lineage.Ancestral.Legacies.AI.States
{
    public class ForageState : IState
    {
        private Pop pop;
        private NavMeshAgent agent;
        private ResourceNode targetNode;
        private float harvestAmount = 1f;

        public void Enter(Pop pop)
        {
            this.pop = pop;
            agent = pop.GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            // Find nearest berry bush
            ResourceNode[] nodes = GameObject.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None);
            float minDist = float.MaxValue;
            foreach (var node in nodes)
            {
                float dist = Vector3.Distance(pop.transform.position, node.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    targetNode = node;
                }
            }
            // TODO: Play foraging animation
            if (targetNode != null)
                agent.SetDestination(targetNode.transform.position);
        }

        public void Tick()
        {
            if (targetNode == null) return;
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Harvest
                if (targetNode.Harvest((int)harvestAmount))
                {
                    // Directly satisfy hunger instead of using inventory
                    var needs = pop.GetComponent<NeedsComponent>();
                    if (needs != null)
                    {
                        needs.SatisfyHunger(20f); // Restore 20 hunger per berry
                        UnityEngine.Debug.Log($"{pop.name} foraged and ate berries!");
                    }
                }
                else
                {
                    // Node depleted, return to idle
                    pop.GetComponent<AI.PopStateMachine>().ChangeState(new IdleState());
                }
            }
        }

        public void Exit()
        {
            // TODO: Cleanup
        }
    }
}
