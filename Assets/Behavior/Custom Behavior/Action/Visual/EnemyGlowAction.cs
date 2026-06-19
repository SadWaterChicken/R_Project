using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "EnemyGlow", story: "[Sprite] glow [Color]", category: "Action", id: "0dcc9c22e14047d273a5f5deb0e96be9")]
public partial class EnemyGlowAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Sprite;
    [SerializeReference] public BlackboardVariable<Color> Color;
    protected override Status OnStart()
    {
        var sprite = Sprite.Value.GetComponent<SpriteRenderer>();
        sprite.color = Color;
        return Status.Success;
    }

}

