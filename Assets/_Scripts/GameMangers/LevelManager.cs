using System.Collections;
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


    static LevelScene _loadedScene = null;
    public UnityEvent<bool> LevelLoaded;
    public UnityEvent<bool> LevelLoadedInverted;
    public static LevelScene LoadedScene
    {
        get => _loadedScene;
        private set
        {
            if (_loadedScene == value) return;
            if (_loadedScene != null)
            {
                UnloadScene(_loadedScene.SceneName);
            }
            _loadedScene = value;
            if (_loadedScene != null)
            {
                LoadScene(_loadedScene.SceneName);
            }
            if (instance) instance.LevelLoaded?.Invoke(_loadedScene != null);
            if (instance) instance.LevelLoadedInverted?.Invoke(_loadedScene == null);
        }
    }

    public static void LoadLevelScene(LevelScene scene)
    {
        LoadedScene = scene;
    }
    static LevelManager instance;
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
        if (IsServer && autoEnterScene != null)
        {
            StartCoroutine(LoadSceneAsync(autoEnterScene));
        }

    }

    IEnumerator LoadSceneAsync(LevelScene scene)
    {
        yield return new WaitForEndOfFrame();
        LoadedScene = scene;
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    {
    }

}