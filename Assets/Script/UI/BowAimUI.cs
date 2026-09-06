using UnityEngine;
using UnityEngine.UI;

public class BowAimUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject crosshairObj;
    public GameObject chargeBarContainer;
    public Image chargeBarFill;

    private PlayerCombatStateMachine playerCombatStateMachine;

    void Start()
    {
        playerCombatStateMachine = FindAnyObjectByType<PlayerCombatStateMachine>();


        if (crosshairObj != null) crosshairObj.SetActive(false);
        if (chargeBarContainer != null) chargeBarContainer.SetActive(false);
    }

    void Update()
    {
        if (playerCombatStateMachine == null) return;

        bool shouldShowUI = playerCombatStateMachine.isCharging || playerCombatStateMachine.isAiming;

        if (crosshairObj != null)
        {
            crosshairObj.SetActive(shouldShowUI);
        }

        if (chargeBarContainer != null)
        {
            chargeBarContainer.SetActive(playerCombatStateMachine.isCharging);
            if (playerCombatStateMachine.isCharging && chargeBarFill != null)
            {
                chargeBarFill.fillAmount = playerCombatStateMachine.currentCharge;
            }
        }
    }
}
