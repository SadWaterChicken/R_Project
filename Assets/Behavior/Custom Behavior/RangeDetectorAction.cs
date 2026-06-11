using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RangeDetector", story: "Update [rangeDetector] and assign [Target]", category: "Action", id: "768befe6fde2ff05e79bfc967db05e2c")]
public partial class RangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<SimpleDetector> RangeDetector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;



    protected override Status OnUpdate()
    {
        Target.Value = RangeDetector.Value.UpdateDetector();
        return Target.Value == null ? Status.Failure : Status.Success;
    }

}

