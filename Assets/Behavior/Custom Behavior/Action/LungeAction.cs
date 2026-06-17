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

    private float LungeDistance = 10f;
    private float LungeSpeed = 10f;
    private float LungeDuration = 0.5f;
    private Vector3 lungeDirection;
    private float minimumLungeDuration = 0.5f;
    private float lungeStartTime;


    private Rigidbody rb;
    public float LungeForce = 10f;
    private float currentForce;
    private Vector3 direction;
    /*
    protected override Status OnStart()
    {
        rb = Self.Value.GetComponent<Rigidbody>();

        direction = (Target.Value.transform.position -
                     Self.Value.transform.position).normalized;

        direction.y = 0f;

        currentForce = LungeForce;
        rb.AddForce(direction * currentForce, ForceMode.Impulse);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        currentForce -= 20f; // decrease each update

        if (currentForce <= 0f || Target.Value == null)
            return Status.Success;

        return Status.Running;
    }
    */
    protected override Status OnStart()
    {
        rb = Self?.Value?.GetComponent<Rigidbody>();

        // If Self or Target is missing, just succeed immediately
        if (rb == null || Target?.Value == null)
        {
            return Status.Success;
        }

        lungeStartTime = Time.time;

        // Safe to use Target.Value now
        lungeDirection = Target.Value.transform.position - Self.Value.transform.position;
        lungeDirection.y = 0f;
        lungeDirection.Normalize();

        rb.linearVelocity = lungeDirection * LungeSpeed;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (rb == null) return Status.Success;

        float elapsed = Time.time - lungeStartTime;

        // Maintain burst for a short duration
        if (elapsed < LungeDuration)
            return Status.Running;

        // Rapid slowdown
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.deltaTime * 12f);

        // Force exit after a max duration
        if (elapsed > LungeDuration + 0.5f || rb.linearVelocity.magnitude <= 0.2f)
        {
            rb.linearVelocity = Vector3.zero;
            return Status.Success;
        }

        return Status.Running;
    }
}

