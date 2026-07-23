using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    private Dictionary<string, Quest> questMap;


    public HashSet<string> completed = new HashSet<string>();

    private void Awake()
    {
        questMap = CreateQuestMap();

        Quest quest = GetQuestById("Collect");
        Debug.Log(quest.info.displayName);
        Debug.Log(quest.info.levelRequirement);
        Debug.Log(quest.state);
        Debug.Log(quest.CurrentStepExists());

    }
    private Dictionary<string, Quest> CreateQuestMap()
    {
        //load all QuestObject from Asset/Resources/QuestObjects
        QuestObject[] allQuest = Resources.LoadAll<QuestObject>("Quests");
        Dictionary<string, Quest> iDtoQuestMap = new Dictionary<string, Quest>();
        foreach (QuestObject questInfo in allQuest)
        {
            if (iDtoQuestMap.ContainsKey(questInfo.id)) 
            { 
                Debug.LogWarning("Dublicate quest id found: " + questInfo.id + ", skipping this quest object");
            }
            iDtoQuestMap.Add(questInfo.id, new Quest(questInfo));
        }
        return iDtoQuestMap;
    }

    //catch error to avoid access quest id that doesnt exist
    private Quest GetQuestById(string id)
    {
        Quest quest = questMap[id];
        if (quest == null)
        {
            Debug.LogError("ID not found in the Quest Map: " + id);
        }
        return quest;
    }
    public bool HasCompleted(string id)
    {
        if (string.IsNullOrEmpty(id)) return true;
        return completed.Contains(id);
    }
    // for debug
    public void Complete(string id) { if (!string.IsNullOrEmpty(id)) completed.Add(id); }
}
