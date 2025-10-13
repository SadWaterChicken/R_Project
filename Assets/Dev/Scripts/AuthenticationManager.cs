using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    [Header("Development Settings")]
    [SerializeField] private bool testModeEnabled = true;
    [SerializeField] private string testDisplayName = "Test Player";

    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private bool isInitialized = false;

    public event Action<FirebaseUser> OnSignInSuccess;
    public event Action<string> OnSignInFailed;
    public event Action OnSignOutSuccess;
    public event Action<FirebaseUser> OnAuthStateChanged;

    public bool IsInitialized => isInitialized;
    public bool IsSignedIn => currentUser != null;
    public FirebaseUser CurrentUser => currentUser;
    public string UserId => currentUser?.UserId;
    public string UserEmail => currentUser?.Email;
    public string UserDisplayName => currentUser?.DisplayName ?? "Player";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AuthenticationManager: Instance created");
        }
        else
        {
            Debug.LogWarning("AuthenticationManager: Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeAuth();
    }

    private void InitializeAuth()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebaseAuth();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                OnSignInFailed?.Invoke($"Firebase initialization failed: {dependencyStatus}");
            }
        });
    }

    private void InitializeFirebaseAuth()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        isInitialized = true;
        Debug.Log("Firebase Auth initialized successfully");
        if (testModeEnabled)
        {
            Debug.Log("Test Mode ENABLED - Will use Anonymous auth for testing");
        }
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != currentUser)
        {
            bool signedIn = currentUser != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && currentUser != null)
            {
                Debug.Log("Signed out");
            }
            currentUser = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log($"Signed in: {currentUser.UserId}");
                OnAuthStateChanged?.Invoke(currentUser);
            }
        }
    }

    private void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
        }
    }

    public async Task<bool> SignInWithGoogleAsync()
    {
        if (!isInitialized)
        {
            Debug.LogError("Auth not initialized!");
            OnSignInFailed?.Invoke("Authentication system not ready");
            return false;
        }

        try
        {
            Debug.Log("Attempting Google Sign-In...");
#if UNITY_EDITOR
            if (testModeEnabled)
            {
                Debug.Log("Using TEST MODE - Anonymous sign in");
                return await SignInTestModeAsync();
            }
            else
            {
                Debug.LogWarning("Google Sign-In not available in Editor. Enable Test Mode to test.");
                OnSignInFailed?.Invoke("Google Sign-In only works on real devices");
                return false;
            }
#elif UNITY_ANDROID
            Debug.LogError("=== GOOGLE SIGN-IN NOT CONFIGURED ===");
            Debug.LogError("To use Google Sign-In on Android, you need:");
            Debug.LogError("1. Install Google Play Games Plugin for Unity");
            Debug.LogError("2. Setup OAuth 2.0 credentials in Firebase Console");
            Debug.LogError("3. See GOOGLE_SIGNIN_SETUP.md for details");
            Debug.LogError("TEMPORARY: Enable Test Mode in Inspector to test");
            OnSignInFailed?.Invoke("Google Sign-In not configured");
            return false;
#elif UNITY_IOS
            Debug.LogError("=== GOOGLE SIGN-IN NOT CONFIGURED ===");
            Debug.LogError("To use Google Sign-In on iOS, you need:");
            Debug.LogError("1. Install Google Sign-In SDK for iOS");
            Debug.LogError("2. Setup URL schemes in Xcode");
            Debug.LogError("3. See GOOGLE_SIGNIN_SETUP.md for details");
            OnSignInFailed?.Invoke("Google Sign-In not configured");
            return false;
#else
            Debug.LogWarning("Platform not supported");
            OnSignInFailed?.Invoke("Google Sign-In only supports Android and iOS");
            return false;
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"Google sign in failed: {e.Message}");
            OnSignInFailed?.Invoke(GetErrorMessage(e));
            return false;
        }
    }

    private async Task<bool> SignInTestModeAsync()
    {
        try
        {
            Debug.Log("===== TEST MODE SIGN IN =====");
            Debug.Log("Signing in with anonymous account for testing...");
            var result = await auth.SignInAnonymouslyAsync();
            currentUser = result.User;
            UserProfile profile = new UserProfile { DisplayName = testDisplayName };
            await currentUser.UpdateUserProfileAsync(profile);
            Debug.Log($"Test sign in successful!");
            Debug.Log($"User ID: {currentUser.UserId}");
            Debug.Log($"Display Name: {currentUser.DisplayName}");
            Debug.Log("===== TEST MODE READY =====");
            OnSignInSuccess?.Invoke(currentUser);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Test sign in failed: {e.Message}");
            OnSignInFailed?.Invoke("Test mode sign in failed");
            return false;
        }
    }

    public void SignOut()
    {
        if (auth != null && currentUser != null)
        {
            Debug.Log($"Signing out user: {currentUser.UserId}");
            auth.SignOut();
            currentUser = null;
            OnSignOutSuccess?.Invoke();
        }
    }

    private string GetErrorMessage(Exception exception)
    {
        string message = exception.Message.ToLower();
        if (message.Contains("network"))
            return "Network connection error";
        if (message.Contains("cancelled") || message.Contains("canceled"))
            return "Sign in was cancelled";
        if (message.Contains("account") && message.Contains("disabled"))
            return "Account has been disabled";
        if (message.Contains("invalid-credential"))
            return "Invalid credentials";
        return "Google sign in failed. Please try again";
    }
}
