using UnityEngine;

public class PlayerSkillCastBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachSkillCastController()
    {
        PlayerCombat playerCombat = FindObjectOfType<PlayerCombat>();
        if (playerCombat == null) return;

        if (playerCombat.GetComponent<PlayerSkillCastController>() == null)
        {
            playerCombat.gameObject.AddComponent<PlayerSkillCastController>();
        }
    }
}
