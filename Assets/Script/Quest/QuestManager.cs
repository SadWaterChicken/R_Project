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

    }

    //Subcribe to all available quest events
    private void OnEnable()
    {
        GameEventsManager.instance.questEvents.onStartQuest += StartQuest;
        GameEventsManager.instance.questEvents.onAdvanceQuest += AdvanceQuest;
        GameEventsManager.instance.questEvents.onFinishQuest += FinishQuest;
    }

    //Unsubcribe to all available quest events
    private void OnDisable()
    {
        GameEventsManager.instance.questEvents.onStartQuest -= StartQuest;
        GameEventsManager.instance.questEvents.onAdvanceQuest -= AdvanceQuest;
        GameEventsManager.instance.questEvents.onFinishQuest -= FinishQuest;

    }

    private void Start()
    {

        foreach (Quest quest in questMap.Values)
        {
            /*// initialize any loaded quest steps
            if (quest.state == QuestState.IN_PROGRESS)
            {
                quest.InstantiateCurrentQuestStep(this.transform);
            }*/
            // broadcast the initial state of all quests on startup
            GameEventsManager.instance.questEvents.QuestStateChange(quest);
        }
    }

    private void StartQuest(string id)
    {
        Debug.Log("Start Quest");
    }
    private void AdvanceQuest(string id)
    {
        Debug.Log("Advance Quest");
    }

    private void FinishQuest(string id)
    {
        Debug.Log("End Quest");
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
