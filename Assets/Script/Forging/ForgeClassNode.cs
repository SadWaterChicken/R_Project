using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào UI Button đại diện cho 1 Lớp Vũ Khí (Weapon Class) trên màn hình Lò Rèn
/// </summary>
public class ForgeClassNode : MonoBehaviour
{
    [Header("Class Data")]
    [Tooltip("Tên của Lớp Vũ Khí (VD: Greatsword, Staff, Bow...)")]
    public string className;
    
    [Header("UI References")]
    public Button nodeButton;
    
    [Header("Tree Connection")]
    [Tooltip("Object cha chứa toàn bộ cây vũ khí của Class này (sẽ hiển thị khi click)")]
    public GameObject weaponTreeContainer; 

    private void OnValidate()
    {
        if (nodeButton == null) nodeButton = GetComponent<Button>();
    }
}
