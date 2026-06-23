using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour
{
    private static CursorManager _instance;
    public static CursorManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<CursorManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CursorManager_Auto");
                    _instance = go.AddComponent<CursorManager>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Event triggered when all UI panels should be closed (e.g. when player moves)
    /// </summary>
    public static event Action OnCloseAllUI;

    private int activeUIPanelsCount = 0;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateCursorState();
    }

    /// <summary>
    /// UI Scripts must call this when they open or close.
    /// </summary>
    public void SetUIOpen(bool isOpen)
    {
        if (isOpen)
        {
            activeUIPanelsCount++;
        }
        else
        {
            activeUIPanelsCount = Mathf.Max(0, activeUIPanelsCount - 1);
        }

        UpdateCursorState();
    }

    /// <summary>
    /// Call this to force all registered UI panels to close.
    /// </summary>
    public void CloseAllUI()
    {
        if (activeUIPanelsCount > 0)
        {
            // Broadcast the event. Subscribers (Inventory, Shop, etc.) should close themselves
            // and call SetUIOpen(false) in their closing logic.
            OnCloseAllUI?.Invoke();
            
            // Failsafe reset
            activeUIPanelsCount = 0;
            UpdateCursorState();
        }
    }

    public bool IsAnyUIOpen()
    {
        return activeUIPanelsCount > 0;
    }

    private void UpdateCursorState()
    {
        if (activeUIPanelsCount > 0)
        {
            // Unlock cursor and make it visible for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Lock cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        // Failsafe: if no UI is open but cursor somehow got unlocked (e.g. Editor interruption),
        // clicking the game screen will re-lock it.
        if (activeUIPanelsCount == 0 && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
