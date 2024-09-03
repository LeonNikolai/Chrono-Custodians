using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : NetworkBehaviour {
    [SerializeField] LevelScene[] _levelScene;
    static LevelScene autoEnterScene = null;
    [SerializeField] LevelScene _autoEnterScene = null;
    LevelScene _loadedScene = null;
    public LevelScene LoadedScene {
        get => _loadedScene;
        private set {
            if(_loadedScene == value) return;
            if(_loadedScene != null)
            {
                Debug.Log("Server Unloaded scene : " + _loadedScene.SceneName);
                UnloadScene(_loadedScene.SceneName);
            }
            _loadedScene = value;
            if(_loadedScene != null)
            {
                Debug.Log("Server Loaded scene : " + _loadedScene.SceneName);
                LoadScene(_loadedScene.SceneName);
            }
        }
    }
    private void Awake() {
        _loadedScene = null;
        if(autoEnterScene == null) autoEnterScene = _autoEnterScene;
    }
    private void LoadScene(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
    private void UnloadScene(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            NetworkManager.Singleton.SceneManager.UnloadScene(scene);
        }
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer && autoEnterScene != null)
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