using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
[DefaultExecutionOrder(-50)]
public class LevelManager : NetworkBehaviour
{
    [SerializeField] LevelScene[] _levelScene;
    static LevelScene autoEnterScene = null;
    [SerializeField] LevelScene _autoEnterScene = null;

    static LevelScene _loadedScene = null;
    public static LevelScene LoadedScene => _loadedScene;
    public UnityEvent<bool> LevelLoaded;
    public UnityEvent<bool> LevelLoadedInverted;
    public static void LoadLevelScene(LevelScene scene)
    {
        if (IsLoading) return;
        if (instance) instance.LoadLevelSceneInternal(scene);
    }
    internal void LoadLevelSceneInternal(LevelScene scene)
    {
        bool isLoaded = _loadedScene == scene;
        if (isLoaded) return;
        StartCoroutine(LoadLevelSceneAsync(scene));
    }

    public static bool IsLoading { get; private set; }
    IEnumerator LoadLevelSceneAsync(LevelScene scene)
    {
        if(InstabilityManager.instance) InstabilityManager.instance.currentLevel = scene.ToString();
        while (IsLoading)
        {
            yield return null;
        }
        if (_loadedScene != null)
        {
            var unloadScene = SceneManager.GetSceneByName(_loadedScene.SceneName);
            if (unloadScene.IsValid() && unloadScene.isLoaded)
            {
                SceneEventProgressStatus status = NetworkManager.Singleton.SceneManager.UnloadScene(unloadScene);
                if (status != SceneEventProgressStatus.Started)
                {
                    Debug.LogWarning($"Failed to unload {unloadScene} " + $"with a {nameof(SceneEventProgressStatus)}: {status}");
                    yield break;
                }
                IsLoading = true;
                NetworkManager.SceneManager.OnUnloadEventCompleted += OnSceneUnloaded;
                while (IsLoading)
                {
                    yield return null;
                }
            }
        }
        _loadedScene = scene;
        if (_loadedScene != null)
        {
            var status = NetworkManager.Singleton.SceneManager.LoadScene(_loadedScene.SceneName, LoadSceneMode.Additive);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {_loadedScene.SceneName} " + $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }
        if (instance) instance.LevelLoaded?.Invoke(_loadedScene != null);
        if (instance) instance.LevelLoadedInverted?.Invoke(_loadedScene == null);
    }

    private void OnSceneUnloaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        IsLoading = false;
        NetworkManager.SceneManager.OnUnloadEventCompleted -= OnSceneUnloaded;
    }


    public static LevelManager instance;
    private void Awake()
    {
        if (LevelLoaded == null) LevelLoaded = new UnityEvent<bool>();
        if (LevelLoaded == null) LevelLoadedInverted = new UnityEvent<bool>();
        instance = this;
        _loadedScene = null;
        if (autoEnterScene == null) autoEnterScene = _autoEnterScene;
    }
    static void LoadScene(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
    static void UnloadScene(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            NetworkManager.Singleton.SceneManager.UnloadScene(scene);
        }
    }

    public override void OnNetworkSpawn()
    {

    }

    public List<string> GetLevelSceneNames()
    {
        List<string> strings = new List<string>();
        foreach(var level in _levelScene)
        {
            strings.Add(level.SceneName);
        }
        return strings;
    }
}