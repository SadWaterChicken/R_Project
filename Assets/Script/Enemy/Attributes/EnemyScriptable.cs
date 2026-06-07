using UnityEngine;
using UnityEngine.AI;




[CreateAssetMenu(fileName = "Enemy Configuration", menuName = "ScriptableObjects/EnemyScriptable Configuration")]
public class EnemyScriptable : ScriptableObject
{
    //Stats
    public int health = 100;

    //NavMesh config
    public float AIUpdateInterval = 0.1f;

    public float acceleration = 8f;
    public float angularSpeed = 120f;

    //-1 mean everything
    public int AreaMask = -1;
    public int AvoidancePriority = 50;
    public float BaseOffset = 0f;
    public float Height = 2f;
    public ObstacleAvoidanceType ObstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
    public float Radius = 0.5f;
    public float speed= 3f;
    public float stoppingDistance = 0.5f;

}
