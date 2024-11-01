using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
[DefaultExecutionOrder(-50)]
public class LevelManager : NetworkBehaviour
{
    [SerializeField] LevelScene[] _levelScene;
    static LevelScene autoEnterScene = null;
    [SerializeField] LevelScene _autoEnterScene = null;

    public LevelScene[] LevelScenes => _levelScene;

    static LevelScene _loadedScene = null;
    public static LevelScene LoadedScene
    {
        get
        {
            return _loadedScene;
        }
        set
        {
            if (_loadedScene == value) return;
            _loadedScene = value;
            if (instance)
            {
                instance.LoadedSceneID.Value = GetSceneID(value);
                instance.LevelLoaded?.Invoke(_loadedScene);
            }
        }
    }
    public NetworkVariable<int> LoadedSceneID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public UnityEvent<LevelScene> LevelLoaded;

    public static HashSet<LevelScene> AllScenes = new HashSet<LevelScene>();
    static List<LevelScene> SceneById = new List<LevelScene>();
    public static LevelScene GetSceneById(int id)
    {
        if (id < 0 || id >= SceneById.Count) return null;
        return SceneById[id] ?? null;
    }
    public static int GetSceneID(LevelScene scene)
    {
        if (scene == null) return -1;
        if (!SceneById.Contains(scene))
        {
            return -1;
        }
        return SceneById.IndexOf(scene);
    }

    public override void OnNetworkSpawn()
    {
        List<LevelScene> scenes = new List<LevelScene>();
        foreach (var level in _levelScene)
        {
            scenes.Add(level);
        }
        scenes.Sort((a, b) => string.Compare(a.SceneName, b.SceneName));
        SceneById.Clear();
        if (!IsHost)
        {
            LoadedSceneID.OnValueChanged += (previous, current) =>
               {
                   LevelScene scene = GetSceneById(current);
                   LoadedScene = scene ?? null;
               };
        }


    }
    public static void LoadLevelScene(LevelScene scene)
    {
        if (IsLoading) return;
        if (instance) instance.LoadLevelSceneInternal(scene);
    }
    internal void LoadLevelSceneInternal(LevelScene scene)
    {
        if (IsHost)
        {
            LoadLevelServer(scene);
        }
        else
        {
            LoadLevelRpc(GetSceneID(scene));
        }
    }
    [Rpc(SendTo.Server)]
    private void LoadLevelRpc(int sceneID)
    {
        LevelScene scene = GetSceneById(sceneID);
        LoadLevelServer(scene);
    }

    public void Reload()
    {
        if (IsHost)
        {
            StartCoroutine(ReloadCoroutine());
            return;
        }
        ReloadServerRpc();
    }

    private IEnumerator ReloadCoroutine()
    {
        IsLoading = true;
        var scene = NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        while (IsLoading)
        {
            yield return new WaitForSeconds(1f);
        }
    }
    [Rpc(SendTo.Server)]
    private void ReloadServerRpc()
    {
        StartCoroutine(ReloadCoroutine());
    }

    private void LoadLevelServer(LevelScene scene)
    {
        bool isLoaded = _loadedScene == scene;
        if (isLoaded) return;
        if (IsLoading) return;
        StartCoroutine(LoadLevelSceneAsync(scene));
    }

    public static bool IsLoading { get; private set; }

    IEnumerator LoadLevelSceneAsync(LevelScene scene)
    {
        if (InstabilityManager.instance && scene) InstabilityManager.instance.currentLevel = scene.ToString();
        while (IsLoading)
        {
            yield return null;
        }
        LevelScene newScene = LoadedScene;
        if (newScene != null)
        {
            var unloadScene = SceneManager.GetSceneByName(newScene.SceneName);
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
        newScene = scene;
        if (newScene != null)
        {
            var status = NetworkManager.Singleton.SceneManager.LoadScene(newScene.SceneName, LoadSceneMode.Additive);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {newScene.SceneName} " + $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }
        LoadedScene = newScene;
    }

    private void OnSceneUnloaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        IsLoading = false;
        NetworkManager.SceneManager.OnUnloadEventCompleted -= OnSceneUnloaded;
    }


    public static LevelManager instance;
    private void Awake()
    {
        foreach (var level in _levelScene)
        {
            AllScenes.Add(level);
        }
        if (LevelLoaded == null) LevelLoaded = new UnityEvent<LevelScene>();
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

    public List<string> GetLevelSceneNames()
    {
        List<string> strings = new List<string>();
        foreach (var level in _levelScene)
        {
            strings.Add(level.SceneName);
        }
        return strings;
    }
}