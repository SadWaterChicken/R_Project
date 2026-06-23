using UnityEngine;

/// <summary>
/// Gắn script này vào Quái vật (Enemy). 
/// Khi quái chết, nó sẽ rớt ra lượng Mastery tương ứng cho vũ khí của người chơi.
/// </summary>
public class EnemyMasteryReward : MonoBehaviour
{
    [Header("Mastery Reward")]
    [Tooltip("Số điểm Mastery vũ khí sẽ nhận được khi giết con quái này")]
    public float masteryGranted = 1f;
}
