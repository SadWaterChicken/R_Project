using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class DungeonManager : MonoBehaviour
{
    private static DungeonManager _instance;
    public static DungeonManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<DungeonManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DungeonManager");
                    _instance = go.AddComponent<DungeonManager>();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    public int floorCount = 0;
    public string returnSceneName = "MapScene";
    public Room currentActiveRoom;

    [Header("UI Feedback")]
    [SerializeField] private Canvas bossOptionsCanvas;
    [SerializeField] private GameObject bossOptionsPanel;
    [SerializeField] private Button goDeeperButton;
    [SerializeField] private Button escapeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI rewardText;

    // Awake: singleton setup and subscribe to global dungeon events
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureBossOptionsUI();
        SetBossOptionsVisible(false);

        DungeonEvents.OnBossDefeated += HandleBossDefeated;
    }

    // OnDestroy: unsubscribe events and clear singleton if needed
    private void OnDestroy()
    {
        DungeonEvents.OnBossDefeated -= HandleBossDefeated;

        if (_instance == this)
            _instance = null;
    }

    // ─── Public API ────────────────────────────────────────────────────────────

    // HandleBossDefeated: show boss options UI when a boss is defeated
    public void HandleBossDefeated(Room room)
    {
        Debug.Log($"[DungeonManager] Boss defeated in room: {room.gameObject.name}!");
        UpdateGoDeeperLabel();
        SetBossOptionsVisible(true);
    }

    // GoDeeper: advance floor and reload scene
    public void GoDeeper()
    {
        floorCount++;
        SetBossOptionsVisible(false);
        Debug.Log($"[DungeonManager] Going deeper — Floor {floorCount}");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // EscapeDungeon: exit dungeon and return to map scene
    public void EscapeDungeon()
    {
        floorCount = 0;
        SetBossOptionsVisible(false);
        Debug.Log("[DungeonManager] Escaped dungeon. Floor count reset.");
        SceneManager.LoadScene(returnSceneName);
    }

    // ─── UI Building ───────────────────────────────────────────────────────────

    // EnsureBossOptionsUI: build boss options UI if not present
    private void EnsureBossOptionsUI()
    {
        if (bossOptionsCanvas != null && bossOptionsPanel != null)
        {
            WireButtons();
            return;
        }

        EnsureEventSystem();

        // Canvas
        GameObject canvasObj = new GameObject("BossOptionsCanvas");
        bossOptionsCanvas = canvasObj.AddComponent<Canvas>();
        bossOptionsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        bossOptionsCanvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);

        // Panel
        bossOptionsPanel = CreatePanel(canvasObj.transform);

        // Texts
        titleText  = CreateTMPText("Title",  bossOptionsPanel.transform, "Boss Defeated! Choose your path:", 20, new Vector2(0f,  80f));
        rewardText = CreateTMPText("Reward", bossOptionsPanel.transform, "You found a powerful artifact!",    16, new Vector2(0f,  35f));

        // Buttons
        goDeeperButton = CreateTMPButton("GoDeeperButton", bossOptionsPanel.transform, "Go Deeper",      new Vector2(0f, -20f));
        escapeButton   = CreateTMPButton("EscapeButton",   bossOptionsPanel.transform, "Escape Dungeon", new Vector2(0f, -70f));

        WireButtons();
    }

    // EnsureEventSystem: ensure an EventSystem exists in the scene
    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null) return;

        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
        DontDestroyOnLoad(eventSystemObj);
    }

    // CreatePanel: create the boss options UI panel under canvas
    private GameObject CreatePanel(Transform canvasTransform)
    {
        GameObject panelObj = new GameObject("BossOptionsPanel");
        panelObj.transform.SetParent(canvasTransform, false);

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        RectTransform rect    = panelObj.GetComponent<RectTransform>();
        rect.sizeDelta        = new Vector2(420f, 270f);
        rect.anchorMin        = new Vector2(0.5f, 0.5f);
        rect.anchorMax        = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        return panelObj;
    }

    // CreateTMPText: helper to make a TextMeshProUGUI text element
    private TextMeshProUGUI CreateTMPText(string objName, Transform parent, string content, float fontSize, Vector2 anchoredPos)
    {
        GameObject textObj = new GameObject(objName);
        textObj.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text      = content;
        tmp.fontSize  = fontSize;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rect    = tmp.GetComponent<RectTransform>();
        rect.sizeDelta        = new Vector2(380f, 36f);
        rect.anchorMin        = new Vector2(0.5f, 0.5f);
        rect.anchorMax        = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;

        return tmp;
    }

    // CreateTMPButton: helper to create a styled TMP-based button
    private Button CreateTMPButton(string objName, Transform parent, string label, Vector2 anchoredPos)
    {
        GameObject buttonObj = new GameObject(objName);
        buttonObj.transform.SetParent(parent, false);

        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        ColorBlock colors       = button.colors;
        colors.highlightedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        colors.pressedColor     = new Color(0.10f, 0.10f, 0.10f, 1f);
        button.colors           = colors;

        RectTransform rect    = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta        = new Vector2(260f, 40f);
        rect.anchorMin        = new Vector2(0.5f, 0.5f);
        rect.anchorMax        = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;

        // TMP Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(buttonObj.transform, false);

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 16f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform labelRect   = tmp.GetComponent<RectTransform>();
        labelRect.anchorMin       = Vector2.zero;
        labelRect.anchorMax       = Vector2.one;
        labelRect.offsetMin       = Vector2.zero;
        labelRect.offsetMax       = Vector2.zero;

        return button;
    }

    // ─── Helpers ───────────────────────────────────────────────────────────────

    // WireButtons: attach click handlers to UI buttons
    private void WireButtons()
    {
        if (goDeeperButton != null)
        {
            goDeeperButton.onClick.RemoveAllListeners();
            goDeeperButton.onClick.AddListener(GoDeeper);
        }

        if (escapeButton != null)
        {
            escapeButton.onClick.RemoveAllListeners();
            escapeButton.onClick.AddListener(EscapeDungeon);
        }

        UpdateGoDeeperLabel();
    }

    // SetBossOptionsVisible: show/hide the boss options UI
    private void SetBossOptionsVisible(bool visible)
    {
        bossOptionsPanel?.SetActive(visible);

        if (bossOptionsCanvas != null)
            bossOptionsCanvas.enabled = visible;
    }

    // UpdateGoDeeperLabel: refresh Go Deeper button label with floor info
    private void UpdateGoDeeperLabel()
    {
        if (goDeeperButton == null) return;

        TextMeshProUGUI label = goDeeperButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = $"Go Deeper  (Floor {floorCount + 1})";
    }
}