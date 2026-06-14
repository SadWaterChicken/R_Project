using UnityEngine;
using Unity.Cinemachine;

public class CameraInputLocker : MonoBehaviour
{
    private CinemachineInputAxisController inputController;

    private void Awake()
    {
        // Try to find the Cinemachine Input Controller on this camera
        inputController = GetComponent<CinemachineInputAxisController>();
        
        if (inputController == null)
        {
            Debug.LogWarning("[CameraInputLocker] No CinemachineInputAxisController found! Make sure this script is attached to your Cinemachine FreeLook Camera.");
        }
    }

    private void Update()
    {
        if (inputController != null)
        {
            // Only enable the camera input when the cursor is locked!
            // This prevents the camera from spinning when you open the inventory and move your mouse.
            inputController.enabled = (Cursor.lockState == CursorLockMode.Locked);
        }
    }
}
