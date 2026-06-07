using UnityEngine;

public enum DungeonDifficultyTier
{
    Easy,
    Normal,
    Hard,
    Impossible
}

public class OverworldDungeonEntrance : MonoBehaviour, IInteractable
{
    public DungeonThemeSetup assignedTheme;
    public string dungeonInstanceID;
    public DungeonDifficultyTier difficulty = DungeonDifficultyTier.Normal;

    public DungeonDifficultyTier GetCurrentDifficulty()
    {
        return difficulty;
    }

    public void Interact()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.EnterDungeon(this);
        }
        else
        {
            Debug.LogError("[DungeonEntrance] No GameStateManager found!");
        }
    }
}
