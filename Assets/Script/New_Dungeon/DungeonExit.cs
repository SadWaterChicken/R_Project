using UnityEngine;

public class DungeonExit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered DungeonExit portal!");
            // Add logic here to load the next level or return to town
        }
    }
}
