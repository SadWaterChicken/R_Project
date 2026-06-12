using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace New_Dungeon
{
    public class EventStructure : MonoBehaviour
    {
        public EventRoomController roomController;

        [Header("Shared UI")]
        public GameObject uiPanel;
        public GameObject backgroundOverlay; 

        [Header("Phase 1: Start Event UI")]
        public GameObject phase1Container; 
        public TextMeshProUGUI p1_HeaderTitle;
        public TextMeshProUGUI p1_Description;
        public GameObject p1_RewardPreview; 
        public Button p1_StartButton; 
        public TextMeshProUGUI p1_StartButtonText; // To show cost on button
        
        [Header("Phase 2: Mid-Event UI")]
        public GameObject phase2Container;
        public TextMeshProUGUI p2_HeaderTitle;
        public TextMeshProUGUI p2_CurrentRewardText;
        public TextMeshProUGUI p2_WarningText; 
        public Button p2_TakeButton; 
        public Button p2_ContinueButton; 
        public TextMeshProUGUI p2_ContinueButtonText; // To show cost on button

        [Header("Animation")]
        public float submergeDepth = -10f;
        public float animationDuration = 1.5f;
        private Vector3 initialPosition;

        private void Start()
        {
            initialPosition = transform.position;

            if (uiPanel != null) uiPanel.SetActive(false);

            // Phase 1 Button
            if (p1_StartButton != null)
            {
                p1_StartButton.onClick.RemoveAllListeners();
                p1_StartButton.onClick.AddListener(OnDoubleClicked);
            }

            // Phase 2 Buttons
            if (p2_ContinueButton != null)
            {
                p2_ContinueButton.onClick.RemoveAllListeners();
                p2_ContinueButton.onClick.AddListener(OnDoubleClicked);
            }

            if (p2_TakeButton != null)
            {
                p2_TakeButton.onClick.RemoveAllListeners();
                p2_TakeButton.onClick.AddListener(OnTakeClicked);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !roomController.isEventRunning)
            {
                if (roomController.currentWave == 0)
                {
                    ShowPhase1();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (uiPanel != null) uiPanel.SetActive(false);
            }
        }

        private void ShowPhase1()
        {
            int cost = GetNextWaveCost();
            int playerCubes = GetPlayerCubes();

            if (phase1Container != null) phase1Container.SetActive(true);
            if (phase2Container != null) phase2Container.SetActive(false);

            if (p1_HeaderTitle != null) 
                p1_HeaderTitle.text = "THỬ THÁCH SINH TỬ";
            
            string desc = "Dâng lên Energy Cube để triệu hồi quái vật.\nHoàn thành đợt để nhận Rương.\nBạn có thể dừng lại chốt lời sau mỗi đợt hoặc tiếp tục thử thách để nhận nhiều rương hơn!";
            if (playerCubes < cost)
            {
                int needed = cost - playerCubes;
                desc += $"\n\n<color=red>Chưa đủ điều kiện! Bạn cần tìm thêm {needed} Energy Cube nữa để bắt đầu.</color>";
            }

            if (p1_Description != null) 
                p1_Description.text = desc;

            if (p1_StartButtonText != null)
                p1_StartButtonText.text = $"BẮT ĐẦU\n(Tiêu hao {cost} Cube)";
            
            if (p1_StartButton != null)
                p1_StartButton.interactable = (playerCubes >= cost);

            if (uiPanel != null) uiPanel.SetActive(true);
        }

        public void ShowMidEventChoice()
        {
            RiseUp(); 

            int cost = GetNextWaveCost();
            int playerCubes = GetPlayerCubes();
            int currentReward = roomController.currentWave;

            if (phase1Container != null) phase1Container.SetActive(false);
            if (phase2Container != null) phase2Container.SetActive(true);

            if (p2_HeaderTitle != null) 
                p2_HeaderTitle.text = "THỬ THÁCH HOÀN THÀNH!";
            
            if (p2_CurrentRewardText != null) 
                p2_CurrentRewardText.text = $"Bạn đã tích lũy được: <color=yellow>{currentReward} RƯƠNG</color>";

            string warning = "Dừng lại để nhận rương an toàn.\nHoặc dâng thêm Cube để khiêu chiến đợt tiếp theo.\n<color=red>Cảnh báo: Độ khó sẽ tăng cao!</color>";
            if (playerCubes < cost)
            {
                int needed = cost - playerCubes;
                warning += $"\n\n<color=red>Chưa đủ điều kiện! Bạn cần tìm thêm {needed} Energy Cube nữa để đi tiếp.</color>";
            }

            if (p2_WarningText != null)
                p2_WarningText.text = warning;

            if (p2_ContinueButtonText != null)
                p2_ContinueButtonText.text = $"ĐI TIẾP\n(Tiêu hao {cost} Cube)";

            if (p2_ContinueButton != null)
                p2_ContinueButton.interactable = (playerCubes >= cost);

            if (uiPanel != null) uiPanel.SetActive(true);
        }

        private int GetPlayerCubes()
        {
            return PlayerStat.Instance != null ? PlayerStat.Instance.currentEnergyCubes : 0;
        }

        private int GetNextWaveCost()
        {
            return 1 + roomController.currentWave;
        }

        public void OnDoubleClicked()
        {
            int cost = GetNextWaveCost();

            if (PlayerStat.Instance != null)
            {
                bool success = PlayerStat.Instance.SpendEnergyCubes(cost);
                if (!success) return; 
            }

            if (uiPanel != null) uiPanel.SetActive(false);

            StartCoroutine(LerpPosition(initialPosition + Vector3.up * submergeDepth));
            roomController.StartNextWave();
        }

        public void OnTakeClicked()
        {
            if (uiPanel != null) uiPanel.SetActive(false);
            roomController.TakeChestsAndEnd();
        }

        public void RiseUp()
        {
            StartCoroutine(LerpPosition(initialPosition));
        }

        private IEnumerator LerpPosition(Vector3 targetPos)
        {
            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / animationDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
        }
    }
}
