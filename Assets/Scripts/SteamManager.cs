#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

[DisallowMultipleComponent]
public sealed class SteamManager : MonoBehaviour
{
#if !DISABLESTEAMWORKS
    static SteamManager _instance;
    static bool _everInitialized;

    bool _initialized;
    SteamAPIWarningMessageHook_t _steamAPIWarningMessageHook;

    public static bool Initialized => GetInstance()._initialized;

    public static SteamManager GetInstance()
    {
        if (_instance == null)
        {
            return new GameObject("SteamManager").AddComponent<SteamManager>();
        }
        else
        {
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void InitOnPlayMode()
    {
        _everInitialized = false;
        _instance = null;
    }

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (_everInitialized)
        {
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }

        DontDestroyOnLoad(gameObject);

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        try
        {
            if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);
            Application.Quit();
            return;
        }

        _initialized = SteamAPI.Init();
        if (!_initialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
            return;
        }

        _everInitialized = true;
    }

    void OnEnable()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        if (!_initialized)
        {
            return;
        }

        if (_steamAPIWarningMessageHook != null)
            return;
        _steamAPIWarningMessageHook = SteamAPIDebugTextHook;
        SteamClient.SetWarningMessageHook(_steamAPIWarningMessageHook);
    }

    void Update()
    {
        if (!_initialized)
        {
            return;
        }

        SteamAPI.RunCallbacks();
    }

    void OnDestroy()
    {
        if (_instance != this)
        {
            return;
        }

        _instance = null;

        if (!_initialized)
        {
            return;
        }

        SteamAPI.Shutdown();
    }

    public void SetRichPresence(string key, string value) => SteamFriends.SetRichPresence(key, value);
#else
    public static bool Initialized {
       get {
          return false;
       }
    }
#endif
}