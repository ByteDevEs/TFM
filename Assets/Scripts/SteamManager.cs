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
    static SteamManager instance;
    static bool everInitialized;

    bool initialized;
    SteamAPIWarningMessageHook_t steamAPIWarningMessageHook;

    public static bool Initialized => GetInstance().initialized;

    public static SteamManager GetInstance()
    {
        if (instance == null)
        {
            return new GameObject("SteamManager").AddComponent<SteamManager>();
        }
        else
        {
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void InitOnPlayMode()
    {
        everInitialized = false;
        instance = null;
    }

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (everInitialized)
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

        initialized = SteamAPI.Init();
        if (!initialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
            return;
        }

        everInitialized = true;
    }

    void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (!initialized)
        {
            return;
        }

        if (steamAPIWarningMessageHook != null)
            return;
        steamAPIWarningMessageHook = SteamAPIDebugTextHook;
        SteamClient.SetWarningMessageHook(steamAPIWarningMessageHook);
    }

    void Update()
    {
        if (!initialized)
        {
            return;
        }

        SteamAPI.RunCallbacks();
    }

    void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        instance = null;

        if (!initialized)
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