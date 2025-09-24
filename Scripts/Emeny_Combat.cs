using UnityEngine;

public class Emeny_Attack : MonoBehaviour
{
public int Damage = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.gameObject.GetComponent<PlayerHealth>().ChangeHealth(-Damage);
    }

    
}
