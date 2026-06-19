using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Quản lý việc phóng to và thu nhỏ giao diện Minimap khi người chơi click chuột vào nó.
/// Cần gắn Script này cùng với đối tượng RawImage hiển thị Minimap trên Canvas HUD.
/// </summary>
public class MinimapUIController : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [Tooltip("Khung chứa Minimap (thường là chính đối tượng RectTransform này)")]
    public RectTransform minimapPanel;

    [Tooltip("Camera của Minimap để thay đổi tầm nhìn (Orthographic Size)")]
    public Camera minimapCamera;

    [Header("Panel Size Settings (Kích thước cửa sổ UI)")]
    public Vector2 smallSize = new Vector2(200f, 200f);
    public Vector2 smallPosition = new Vector2(-20f, -20f); // Ở góc trên bên phải
    public Vector2 largeSize = new Vector2(500f, 500f);
    public Vector2 largePosition = new Vector2(0f, 0f); // Ở chính giữa màn hình

    [Header("Camera Zoom Settings (Tầm nhìn camera)")]
    public float smallCamSize = 15f;  // Nhìn gần khi thu nhỏ
    public float largeCamSize = 35f;  // Nhìn xa rộng hơn khi phóng to

    [Header("Anchor Settings (Điểm neo giữ UI)")]
    [Tooltip("Điểm neo khi thu nhỏ (mặc định Top-Right: 1, 1)")]
    public Vector2 smallAnchorMin = new Vector2(1f, 1f);
    public Vector2 smallAnchorMax = new Vector2(1f, 1f);
    public Vector2 smallPivot = new Vector2(1f, 1f);

    [Tooltip("Điểm neo khi phóng to (mặc định Center: 0.5, 0.5)")]
    public Vector2 largeAnchorMin = new Vector2(0.5f, 0.5f);
    public Vector2 largeAnchorMax = new Vector2(0.5f, 0.5f);
    public Vector2 largePivot = new Vector2(0.5f, 0.5f);

    private bool isLarge = false;

    private void Start()
    {
        if (minimapPanel == null)
        {
            minimapPanel = GetComponent<RectTransform>();
        }

        // Đặt trạng thái ban đầu là thu nhỏ
        SetMinimapState(false);
    }

    /// <summary>
    /// Bắt sự kiện click chuột của Unity EventSystem
    /// Lưu ý: Đối tượng này cần phải bật thuộc tính 'Raycast Target' trong component RawImage/Image
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleMinimap();
    }

    /// <summary>
    /// Đảo trạng thái phóng to/thu nhỏ
    /// </summary>
    public void ToggleMinimap()
    {
        isLarge = !isLarge;
        SetMinimapState(isLarge);
    }

    private void SetMinimapState(bool state)
    {
        if (minimapPanel == null) return;

        if (state)
        {
            // Trạng thái phóng to (ở giữa màn hình)
            minimapPanel.anchorMin = largeAnchorMin;
            minimapPanel.anchorMax = largeAnchorMax;
            minimapPanel.pivot = largePivot;
            minimapPanel.sizeDelta = largeSize;
            minimapPanel.anchoredPosition = largePosition;

            if (minimapCamera != null)
            {
                minimapCamera.orthographicSize = largeCamSize;
            }
        }
        else
        {
            // Trạng thái thu nhỏ (ở góc màn hình)
            minimapPanel.anchorMin = smallAnchorMin;
            minimapPanel.anchorMax = smallAnchorMax;
            minimapPanel.pivot = smallPivot;
            minimapPanel.sizeDelta = smallSize;
            minimapPanel.anchoredPosition = smallPosition;

            if (minimapCamera != null)
            {
                minimapCamera.orthographicSize = smallCamSize;
            }
        }
    }
}
