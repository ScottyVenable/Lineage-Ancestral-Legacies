using UnityEngine;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Systems.Inventory;
using Lineage.Ancestral.Legacies.Systems.Resource;

namespace Lineage.Ancestral.Legacies.AI.States
{
    public class ForageState : IState
    {
        private Pop pop;
        private ResourceNode targetNode;
        private float harvestAmount = 1f;

        public void Enter(Pop pop)
        {
            this.pop = pop;
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
        }

        public void Tick()
        {
            if (targetNode == null) return;
            // Move towards target
            pop.transform.position = Vector3.MoveTowards(pop.transform.position, targetNode.transform.position, Time.deltaTime);
            if (Vector3.Distance(pop.transform.position, targetNode.transform.position) < 0.5f)
            {
                // Harvest
                if (targetNode.Harvest((int)harvestAmount))
                {
                    var inv = pop.GetComponent<InventoryComponent>();
                    inv.AddItem("berries", (int)harvestAmount);
                    // TODO: Display floating text
                }
                else
                {
                    // Node depleted, exit state
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
