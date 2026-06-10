using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckDetector", story: "Check if [Detector] has a [target]", category: "Action", id: "ae2302714492ffe583e150562082aea2")]
public partial class CheckDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<SimpleDetector> Detector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnUpdate()
    {
        return Detector.Value.currentTarget == null ? Status.Failure : Status.Success;
    }

}

