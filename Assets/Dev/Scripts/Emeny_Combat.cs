using UnityEngine;

public class Emeny_Attack : MonoBehaviour
{
    public int Damage = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Sử dụng PlayerData thay vì PlayerHealth
        PlayerData playerData = collision.gameObject.GetComponent<PlayerData>();
        if (playerData != null)
        {
            playerData.TakeDamage(Damage);
        }
    }
}
