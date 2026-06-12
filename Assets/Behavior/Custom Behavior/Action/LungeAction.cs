using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Lunge", story: "[Self] Lunges at [Target]", category: "Action", id: "1175a4e96cebfb404587e8cc0fc0ca72")]
public partial class LungeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    [SerializeReference] public BlackboardVariable<float> LungeDistance = new(10f);
    [SerializeReference] public BlackboardVariable<float> LungeSpeed = new(10f);


    public float LungeForce = 10f;

    protected override Status OnStart()
    {
        if (Self.Value == null || Target.Value == null)
            return Status.Failure;

        Rigidbody rb = Self.Value.GetComponent<Rigidbody>();

        if (rb == null)
            return Status.Failure;

        Vector3 direction =
            Target.Value.transform.position - Self.Value.transform.position;

        direction.y = 0f;
        direction.Normalize();

        rb.AddForce(direction * LungeForce, ForceMode.Impulse);

        return Status.Success;
    }

}

