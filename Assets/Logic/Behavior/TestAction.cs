using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Lineage.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Test Action", story: "Test Unity Behavior system", category: "Test", id: "test_action_001")]
    public partial class TestAction : Action
    {
        protected override Status OnStart()
        {
            Debug.Log.Debug("Test Action Started");
            return Status.Success;
        }

        protected override Status OnUpdate()
        {
            return Status.Success;
        }

        protected override void OnEnd()
        {
            Debug.Log.Debug("Test Action Ended");
        }
    }
}
