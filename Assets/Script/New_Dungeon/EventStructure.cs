using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace New_Dungeon
{
    public class EventStructure : MonoBehaviour
    {
        public EventRoomController roomController;

        [Header("UI References")]
        public GameObject uiPanel;
        public Slider cubeSlider;
        public Button plusButton;
        public Button minusButton;
        public Button activateButton;
        public TextMeshProUGUI cubeAmountText;
        
        private int currentCubesToOffer = 0;
        private int maxCubesPlayerHas = 0; // Set this by reading player inventory

        private void Start()
        {
            if (uiPanel != null)
                uiPanel.SetActive(false);

            if (cubeSlider != null)
                cubeSlider.onValueChanged.AddListener(OnSliderValueChanged);

            if (plusButton != null)
                plusButton.onClick.AddListener(IncreaseOffer);

            if (minusButton != null)
                minusButton.onClick.AddListener(DecreaseOffer);

            if (activateButton != null)
                activateButton.onClick.AddListener(ActivateEvent);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !roomController.isEventRunning)
            {
                // In reality, get real amount from MainInventory
                // maxCubesPlayerHas = MainInventory.Instance.GetItemCount("item_energy_cube");
                maxCubesPlayerHas = 100; // Mock for testing

                currentCubesToOffer = 0;
                UpdateUI();
                
                if (uiPanel != null)
                    uiPanel.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (uiPanel != null)
                    uiPanel.SetActive(false);
            }
        }

        private void UpdateUI()
        {
            if (cubeSlider != null)
            {
                cubeSlider.maxValue = maxCubesPlayerHas;
                cubeSlider.value = currentCubesToOffer;
            }

            if (cubeAmountText != null)
            {
                cubeAmountText.text = currentCubesToOffer.ToString();
            }
        }

        public void OnSliderValueChanged(float value)
        {
            currentCubesToOffer = Mathf.RoundToInt(value);
            UpdateUI();
        }

        public void IncreaseOffer()
        {
            if (currentCubesToOffer < maxCubesPlayerHas)
            {
                currentCubesToOffer++;
                UpdateUI();
            }
        }

        public void DecreaseOffer()
        {
            if (currentCubesToOffer > 0)
            {
                currentCubesToOffer--;
                UpdateUI();
            }
        }

        public void ActivateEvent()
        {
            if (currentCubesToOffer > 0)
            {
                // Deduct from inventory
                // MainInventory.Instance.RemoveItem("item_energy_cube", currentCubesToOffer);

                if (uiPanel != null)
                    uiPanel.SetActive(false);

                // Start Event
                roomController.StartEvent(currentCubesToOffer);

                // Submerge structure
                transform.position = new Vector3(transform.position.x, -10f, transform.position.z);
            }
        }

        public void RiseUp()
        {
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        }
    }
}
