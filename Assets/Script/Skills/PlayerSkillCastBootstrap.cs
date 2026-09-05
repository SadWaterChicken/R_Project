using UnityEngine;

public class PlayerSkillCastBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachSkillCastController()
    {
        GameObject bootstrapObject = new GameObject("[Skill] Player Skill Cast Bootstrap");
        DontDestroyOnLoad(bootstrapObject);
        bootstrapObject.AddComponent<PlayerSkillCastBootstrap>();

        // Tích hợp hệ thống Sword Skill (Skill Tree & Mastery)
        bootstrapObject.AddComponent<SwordSkillTreeManager>();
        bootstrapObject.AddComponent<SwordMasteryTracker>();

        // Tích hợp hệ thống quản lý danh sách Skill (Mới)
        bootstrapObject.AddComponent<PlayerSkillManager>();

    }

    private void Update()
    {
        PlayerCombatStateMachine playerCombatStateMachine = FindAnyObjectByType<PlayerCombatStateMachine>();
        if (playerCombatStateMachine == null) return;

        if (playerCombatStateMachine.GetComponent<PlayerSkillCastController>() == null)
        {
            playerCombatStateMachine.gameObject.AddComponent<PlayerSkillCastController>();
        }

        Destroy(this);
    }
}
