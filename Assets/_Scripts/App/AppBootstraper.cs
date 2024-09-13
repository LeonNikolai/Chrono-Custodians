using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

// This class is used to register systems to the player loop and other application initialization
public static class AppBootstraper
{
    public static PlayerLoopSystem applicationLoop;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void BeforeSplashScreen()
    {
        App.OnApplicationAwake();
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AfterSceneLoad()
    {
        App.OnApplicationStart();
    }

    public static void CreateAppBehaviour()
    {
        var AppSingleton = new GameObject("[GAME] APP", new Type[] {
            typeof(AppBehaviour)
        }) {
            hideFlags = HideFlags.HideInHierarchy,
            transform = {
                hideFlags = HideFlags.HideInHierarchy,
            },
        };
        GameObject.DontDestroyOnLoad(AppSingleton);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void OnAssemblyLoaded()
    {
        PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        applicationLoop = new PlayerLoopSystem()
        {
            type = typeof(App),
            updateDelegate = App.OnFrame,
            subSystemList = null
        };
        if (!PlayerLoopUtils.InsertSystem<Update>(ref playerLoop, applicationLoop, 0))
        {
            Debug.LogError("Failed to insert application system");
            return;
        }
        PlayerLoop.SetPlayerLoop(playerLoop);

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

    }
    #if UNITY_EDITOR
    private static void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.ExitingPlayMode)
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopUtils.RemoveSystem<Update>(ref playerLoop, applicationLoop);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }
    }
#endif
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplicationSceneLoad()
    {
        CreateAppBehaviour();
    }

}