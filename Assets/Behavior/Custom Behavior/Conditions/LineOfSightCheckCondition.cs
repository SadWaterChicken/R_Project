using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line of sight check", story: "Check [Target] with line of sight [Detector]", category: "Conditions", id: "ce1c326c714640c87814663d3bf7745c")]
public partial class LineOfSightCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<SimpleDetector> Detector;

    public override bool IsTrue()
    {
        return Detector.Value.DetectingSight(Target.Value) != null;
    }

}
