using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string sceneName = "SampleScene";
    [SerializeField] private string targetSpawnPointName = "PlayerSpawnPoint";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Lưu tên spawn point vào bộ nhớ tạm
            PlayerSpawnManager.nextSpawnPointName = targetSpawnPointName;

            // Load scene đích
            SceneManager.LoadScene(sceneName);
        }
    }
}
