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
        private set
        {
            if (_loadedScene == value) return;
            _loadedScene = value;
            if (instance)
            {
                instance.LoadedLevel.Value = _loadedScene?.GetNetworklevel() ?? LevelSceneRefference.None;
                instance.OnLevelLoaded?.Invoke(_loadedScene);
            }
        }
    }
    public NetworkVariable<LevelSceneRefference> LoadedLevel = new NetworkVariable<LevelSceneRefference>(LevelSceneRefference.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public UnityEvent<LevelScene> OnLevelLoaded = new UnityEvent<LevelScene>();
    static List<LevelScene> sceneById = new List<LevelScene>();

    public override void OnNetworkSpawn()
    {
        List<LevelScene> scenes = new List<LevelScene>();
        foreach (var level in _levelScene)
        {
            scenes.Add(level);
        }
        scenes.Sort((a, b) => string.Compare(a.FirstSceneName, b.FirstSceneName));
        sceneById.Clear();
        if (!IsHost)
        {
            LoadedLevel.OnValueChanged += (previous, current) => LoadedScene = current.Refference;
        }


    }
    public static void LoadLevelScene(LevelScene scene, int index = -1)
    {
        if (IsLoading) return;
        if (instance) instance.LoadLevelSceneInternal(scene, index);
    }
    internal void LoadLevelSceneInternal(LevelScene scene, int index = -1)
    {
        if (IsHost)
        {
            LoadLevelServer(scene, index);
        }
        else
        {
            LoadLevelRpc(scene, index);
        }
    }
    [Rpc(SendTo.Server)]
    private void LoadLevelRpc(LevelSceneRefference sceneRefference, int index)
    {
        LevelScene scene = sceneRefference.Refference;
        LoadLevelServer(scene, index);
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

    private void LoadLevelServer(LevelScene scene, int index = -1)
    {
        bool isLoaded = _loadedScene == scene;
        if (isLoaded) return;
        if (IsLoading) return;
        StartCoroutine(LoadLevelSceneAsync(scene, index));
    }

    public static bool IsLoading { get; private set; }
    public string lastLoadedSceneName = "";
    IEnumerator LoadLevelSceneAsync(LevelScene scene, int index = -1)
    {
        while (IsLoading)
        {
            yield return null;
        }
        LevelScene newScene = LoadedScene;
        if (newScene != null)
        {
            var sceneName = lastLoadedSceneName == "" ? newScene.FirstSceneName : lastLoadedSceneName;
            var unloadScene = SceneManager.GetSceneByName(sceneName);
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
            lastLoadedSceneName = newScene.SceneNameFromIndex(index);
            var status = NetworkManager.Singleton.SceneManager.LoadScene(lastLoadedSceneName, LoadSceneMode.Additive);

            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {lastLoadedSceneName} with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }
        else
        {
            lastLoadedSceneName = "";
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
            if (level == null) continue;
        }
        if (OnLevelLoaded == null) OnLevelLoaded = new UnityEvent<LevelScene>();
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
            strings.Add(level.FirstSceneName);
        }
        return strings;
    }
}