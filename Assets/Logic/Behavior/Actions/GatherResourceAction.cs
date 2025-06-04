using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Lineage.Ancestral.Legacies.Entities;
using Lineage.Ancestral.Legacies.Components;
using Lineage.Ancestral.Legacies.Database;
using Lineage.Ancestral.Legacies.Debug;
namespace Lineage.Ancestral.Legacies.Behavior.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Gather Resource", story: "[Self] gathers from [TargetResource] for [GatherTime] seconds", category: "Resource", id: "gather_resource_001")]
    public partial class GatherResourceAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<GameObject> TargetResource;
        [SerializeReference] public BlackboardVariable<float> GatherTime = new(3f);
        [SerializeReference] public BlackboardVariable<int> GatherAmount = new(1);

        private float gatherStartTime;
        private bool isGathering;        private Lineage.Ancestral.Legacies.Entities.Pop pop;
        private Lineage.Ancestral.Legacies.Components.EntityDataComponent entityData;

        protected override Status OnStart()
        {
            if (Self.Value == null || TargetResource.Value == null)
            {
                Debug.LogError("GatherResource: Self or TargetResource is null");
                return Status.Failure;
            }

            pop = Self.Value.GetComponent<Pop>();
            entityData = Self.Value.GetComponent<EntityDataComponent>();

            if (pop == null || entityData == null)
            {
                Debug.LogError("GatherResource: Required components not found on Self");
                return Status.Failure;
            }

            // Check if we're close enough to the resource
            float distance = Vector3.Distance(Self.Value.transform.position, TargetResource.Value.transform.position);
            if (distance > 3f) // Interaction range
            {
                Debug.LogWarning("GatherResource: Too far from resource to gather");
                return Status.Failure;
            }

            // Start gathering
            gatherStartTime = Time.time;
            isGathering = true;

            // Set entity state to gathering
            if (entityData != null)
            {
                entityData.ChangeState(State.ID.Gathering);
            }

            // Play gathering animation if available
            if (pop.Animator != null)
            {
                pop.Animator.SetBool("IsGathering", true);
            }

            Debug.Log($"{pop.popName} started gathering from {TargetResource.Value.name}");
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!isGathering) return Status.Failure;

            // Check if gathering time has elapsed
            if (Time.time - gatherStartTime >= GatherTime.Value)
            {
                // Perform the actual gathering
                bool gatherSuccess = PerformGathering();
                
                if (gatherSuccess)
                {
                    Debug.Log($"{pop.popName} successfully gathered from {TargetResource.Value.name}");
                    return Status.Success;
                }
                else
                {
                    Debug.LogWarning($"{pop.popName} failed to gather from {TargetResource.Value.name}");
                    return Status.Failure;
                }
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            isGathering = false;

            // Stop gathering animation
            if (pop?.Animator != null)
            {
                pop.Animator.SetBool("IsGathering", false);
            }

            // Change entity state back to idle
            if (entityData != null)
            {
                entityData.ChangeState(State.ID.Idle);
            }
        }

        private bool PerformGathering()
        {
            // Try to get ResourceSource component
            var resourceSource = TargetResource.Value.GetComponent<ResourceSource>();
            if (resourceSource != null)
            {
                return resourceSource.GatherResource(pop, GatherAmount.Value);
            }

            // Fallback: basic resource gathering
            // Restore some hunger/thirst if it's a food/water resource
            string resourceTag = TargetResource.Value.tag;
            
            switch (resourceTag.ToLower())
            {
                case "food":
                    entityData.ModifyStat(Stat.ID.Hunger, 25f);
                    Debug.Log($"{pop.popName} consumed food, restored hunger");
                    return true;
                    
                case "water":
                    entityData.ModifyStat(Stat.ID.Thirst, 30f);
                    Debug.Log($"{pop.popName} drank water, restored thirst");
                    return true;
                    
                default:
                    // Generic gatherable resource
                    Debug.Log($"{pop.popName} gathered generic resource: {resourceTag}");
                    return true;
            }
        }
    }
}
