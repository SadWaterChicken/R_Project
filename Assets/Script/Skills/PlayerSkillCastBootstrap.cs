using UnityEngine;

public class PlayerSkillCastBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachSkillCastController()
    {
        GameObject bootstrapObject = new GameObject("[Skill] Player Skill Cast Bootstrap");
        DontDestroyOnLoad(bootstrapObject);
        bootstrapObject.AddComponent<PlayerSkillCastBootstrap>();
    }

    private void Update()
    {
        PlayerCombat playerCombat = FindObjectOfType<PlayerCombat>();
        if (playerCombat == null) return;

        if (playerCombat.GetComponent<PlayerSkillCastController>() == null)
        {
            playerCombat.gameObject.AddComponent<PlayerSkillCastController>();
        }

        Destroy(gameObject);
    }
}
