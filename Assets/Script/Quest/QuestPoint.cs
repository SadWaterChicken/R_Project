using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class QuestPoint : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private QuestObject questInfoForPoint;

    [Header("Config")]
    [SerializeField] private bool startPoint = true;
    [SerializeField] private bool finishPoint = true;

    private string questId;
    private QuestState currentQuestState;

    private void Awake()
    {
        questId = questInfoForPoint.id;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
    }
    private void Update()
    {
        SubmitPressed();
    }
    private void SubmitPressed()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!playerIsNear)
            {
                return;
            }
            GameEventsManager.instance.questEvents.StartQuest(questId);
            GameEventsManager.instance.questEvents.AdvanceQuest(questId);
            GameEventsManager.instance.questEvents.FinishQuest(questId);

            if (currentQuestState.Equals(QuestState.CAN_START) && startPoint)
            {
                GameEventsManager.instance.questEvents.StartQuest(questId);
            }
            else if (currentQuestState.Equals(QuestState.CAN_FINISH) && finishPoint)
            {
                GameEventsManager.instance.questEvents.FinishQuest(questId);
            }
        }

        
    }
    private void QuestStateChange(Quest quest)
    {
        // only update the quest state if this point has the corresponding quest
        if (quest.info.id.Equals(questId))
        {
            currentQuestState = quest.state;
            Debug.Log($"Quest with Id; "+ questId +"update to  state" + currentQuestState);
        }
    }
    private bool playerIsNear = false;
    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player"))
        {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        if (otherCollider.CompareTag("Player"))
        {
            playerIsNear = false;
        }
    }
}
