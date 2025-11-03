using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Firebase.Auth;

/// <summary>
/// Login UI Controller - Simplified Google Sign-In Only
/// Manages the login screen with a single "Sign in with Google" button
/// Supports Test Mode for development in Unity Editor
/// </summary>
public class LoginUIController : MonoBehaviour
{
    #region Serialized Fields

    [Header("UI References")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private Button googleSignInButton;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Settings")]
    [SerializeField] private float errorMessageDuration = 3f;
    [SerializeField] private string defaultInfoMessage = "Sign in with Google to save your progress";

    #endregion

    #region Private Fields

    private bool isProcessing = false;
    private Coroutine errorCoroutine;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        SetupUI();
        SubscribeToAuthEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromAuthEvents();
    }

    #endregion

    #region Initialization

    private void SetupUI()
    {
        // Setup button listeners
        if (googleSignInButton != null)
        {
            googleSignInButton.onClick.AddListener(OnGoogleSignInClicked);
        }
        else
        {
            Debug.LogWarning("LoginUI: Google Sign-In button not assigned!");
        }

        // Initial UI state
        ShowLoginPanel();
        HideError();

        // Set info text
        if (infoText != null)
        {
            infoText.text = defaultInfoMessage;
        }
    }

    private void SubscribeToAuthEvents()
    {
        if (AuthenticationManager.Instance != null)
        {
            AuthenticationManager.Instance.OnSignInSuccess += OnSignInSuccess;
            AuthenticationManager.Instance.OnSignInFailed += OnSignInFailed;
        }
        else
        {
            Debug.LogWarning("LoginUI: AuthenticationManager instance not found!");
        }
    }

    private void UnsubscribeFromAuthEvents()
    {
        if (AuthenticationManager.Instance != null)
        {
            AuthenticationManager.Instance.OnSignInSuccess -= OnSignInSuccess;
            AuthenticationManager.Instance.OnSignInFailed -= OnSignInFailed;
        }
    }

    #endregion

    #region Panel Management

    private void ShowLoginPanel()
    {
        SetPanelActive(loginPanel, true);
        SetPanelActive(loadingPanel, false);
    }

    private void ShowLoadingPanel(string message)
    {
        SetPanelActive(loadingPanel, true);
        SetPanelActive(loginPanel, false);

        if (loadingText != null)
        {
            loadingText.text = message;
        }
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    #endregion

    #region Button Handlers

    private async void OnGoogleSignInClicked()
    {
        // Prevent multiple clicks
        if (isProcessing)
        {
            Debug.LogWarning("LoginUI: Already processing sign in request");
            return;
        }

        // Validate AuthenticationManager
        if (AuthenticationManager.Instance == null)
        {
            ShowError("Authentication system not initialized");
            Debug.LogError("LoginUI: AuthenticationManager instance is null!");
            return;
        }

        // Start sign-in process
        isProcessing = true;
        HideError();
        ShowLoadingPanel("Signing in with Google...");

        Debug.Log("LoginUI: User clicked Google Sign-In button");

        // Attempt sign-in
        bool success = await AuthenticationManager.Instance.SignInWithGoogleAsync();

        // Handle failure (success is handled by event)
        if (!success)
        {
            ShowLoginPanel();
            isProcessing = false;
        }
    }

    #endregion

    #region Auth Event Handlers

    private void OnSignInSuccess(FirebaseUser user)
    {
        isProcessing = false;

        string displayName = user?.DisplayName ?? "Player";
        string userId = user?.UserId ?? "Unknown";

        Debug.Log($"LoginUI: Sign-in successful! User: {displayName} (ID: {userId})");

        // Update loading text with welcome message
        if (loadingText != null)
        {
            loadingText.text = $"Welcome, {displayName}!";
        }

        // MenuController will handle scene transition
    }

    private void OnSignInFailed(string errorMessage)
    {
        isProcessing = false;

        Debug.LogError($"LoginUI: Sign-in failed - {errorMessage}");

        ShowLoginPanel();
        ShowError(errorMessage);
    }

    #endregion

    #region Error Display

    private void ShowError(string message)
    {
        if (errorText == null)
        {
            Debug.LogWarning("LoginUI: Error text component not assigned!");
            return;
        }

        // Stop previous error hide coroutine if any
        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
        }

        // Show error
        errorText.text = message;
        errorText.gameObject.SetActive(true);

        // Auto-hide after duration
        errorCoroutine = StartCoroutine(HideErrorAfterDelay());

        Debug.LogError($"LoginUI: Error displayed - {message}");
    }

    private void HideError()
    {
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }

        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
            errorCoroutine = null;
        }
    }

    private IEnumerator HideErrorAfterDelay()
    {
        yield return new WaitForSeconds(errorMessageDuration);
        HideError();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Show the login screen (called from MenuController or other scripts)
    /// </summary>
    public void Show()
    {
        ShowLoginPanel();
        HideError();
        isProcessing = false;
    }

    /// <summary>
    /// Hide the login screen
    /// </summary>
    public void Hide()
    {
        SetPanelActive(loginPanel, false);
        SetPanelActive(loadingPanel, false);
        HideError();
    }

    #endregion
}
