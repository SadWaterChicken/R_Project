using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float defaultFOV = 60f;
    public float zoomFOV = 30f;
    public float zoomSpeed = 5f;

    private Camera mainCam;
    private PlayerCombatStateMachine playerCombatStateMachine;

    void Start()
    {
        mainCam = Camera.main;
        playerCombatStateMachine = FindAnyObjectByType<PlayerCombatStateMachine>();
    }

    void Update()
    {
        if (mainCam != null && playerCombatStateMachine != null)
        {
            float targetFOV = playerCombatStateMachine.isAiming ? zoomFOV : defaultFOV;
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        }
    }
}
