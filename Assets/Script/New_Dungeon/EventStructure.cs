using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace New_Dungeon
{
    public class EventStructure : MonoBehaviour, IInteractable
    {
        public EventRoomController roomController;

        [Header("UI Elements")]
        public GameObject mainPanel;  // Screen space panel
        public TextMeshProUGUI messageText; 
        public Button startButton;

        [Header("Animation")]
        public float submergeDepth = -10f;
        public float animationDuration = 1.5f;
        private Vector3 initialPosition;

        private void Start()
        {
            initialPosition = transform.position;

            if (mainPanel != null) mainPanel.SetActive(false);

            if (messageText != null)
            {
                // Automatically fix the massive font size issue
                messageText.enableAutoSizing = true;
                messageText.fontSizeMax = 8; 
                messageText.alignment = TextAlignmentOptions.Center;
            }

            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(OnStartClicked);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Removed hintCanvas logic, PlayerController handles it now.
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Auto close UI when walking away
                if (mainPanel != null && mainPanel.activeSelf) 
                {
                    mainPanel.SetActive(false);
                    
                    // Lock cursor back when walking away
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        public void Interact()
        {
            if (roomController.isEventRunning) return;
            
            if (mainPanel != null) 
            {
                mainPanel.SetActive(true);
                UpdateUIState();
                
                // Unlock cursor so player can click UI
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void ShowMidEventChoice()
        {
            RiseUp(); 
            // The panel doesn't auto-open anymore. The player has to press F to interact with the structure again.
            // The global PlayerController hint UI will automatically handle showing "F" if they are in range.
        }

        private void UpdateUIState()
        {
            if (messageText == null) return;
            
            int cost = GetNextWaveCost();
            
            if (roomController.currentWave == 0)
            {
                messageText.text = $"Start Trial?\nCost: {cost} Energy Cubes";
            }
            else
            {
                messageText.text = $"Wave {roomController.currentWave} Completed!\nReward added to Sack.\nNext Wave Cost: {cost} Energy Cubes";
            }
        }

        private int GetNextWaveCost()
        {
            return 1 + roomController.currentWave;
        }

        private int GetPlayerCubes()
        {
            return PlayerStat.Instance != null ? PlayerStat.Instance.currentEnergyCubes : 0;
        }

        public void OnStartClicked()
        {
            int cost = GetNextWaveCost();
            int playerCubes = GetPlayerCubes();

            if (playerCubes < cost)
            {
                if (messageText != null)
                {
                    messageText.text = "You don't have enough Energy cube";
                }
                return;
            }

            // Spend cubes
            if (PlayerStat.Instance != null)
            {
                bool success = PlayerStat.Instance.SpendEnergyCubes(cost);
                if (!success) return; 
            }

            if (mainPanel != null) mainPanel.SetActive(false);

            // Lock cursor back when starting event
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            StartCoroutine(LerpPosition(initialPosition + Vector3.up * submergeDepth));
            roomController.StartNextWave();
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
