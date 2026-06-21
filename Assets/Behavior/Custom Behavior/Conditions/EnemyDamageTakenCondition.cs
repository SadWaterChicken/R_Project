using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "EnemyDamageTaken", story: "[Self] have Taken Damage check by [detector]", category: "Conditions", id: "05a6b963ca2ce8981e4f5248fa9bcf05")]
public partial class EnemyDamageTakenCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<SimpleDetector> Detector;

    public override bool IsTrue()
    {
        return Detector.Value.HasTakenDamage;
    }

}
