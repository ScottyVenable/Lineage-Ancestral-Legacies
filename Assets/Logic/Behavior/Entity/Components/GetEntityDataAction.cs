using Lineage.Ancestral.Legacies.Entities;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetEntityData", story: "Get [data] from [entity]", category: "Action", id: "ba1582e33b393e4b0e607d66b935a972")]

/// <summary>
/// Action to retrieve data from a specified entity.
/// </summary>
public partial class GetEntityDataAction : Action
{
    [SerializeReference] public BlackboardVariable<Pop> Data;
    [SerializeReference] public BlackboardVariable<GameObject> Entity;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

