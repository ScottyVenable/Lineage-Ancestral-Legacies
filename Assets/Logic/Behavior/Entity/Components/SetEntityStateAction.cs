using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetEntityState", story: "[Agent] State becomes [state]", category: "Action", id: "0b3064ebba1aa98020e94dc57152d5bf")]
/// <summary>
/// Action to set the state of a specified entity.
/// </summary>
public partial class SetEntityStateAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<State> State;
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

