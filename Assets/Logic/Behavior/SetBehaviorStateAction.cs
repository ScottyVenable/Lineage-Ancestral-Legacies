using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetBehaviorState", story: "Set [CurrentState] to [State]", category: "Entity", id: "b89f8305c3aad3585faf1da19a0977c4")]
public partial class SetBehaviorStateAction : Action
{
    [SerializeReference] public BlackboardVariable<BehaviorState> CurrentState;
    [SerializeReference] public BlackboardVariable<BehaviorState> State;

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

