using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string gameScene = "GameLobby";
    public static string GameScene = "GameLobby";
    void Awake()
    {
        GameScene = gameScene;
    }

    public void StartClientGame()
    {
        if(NetworkManager.Singleton.IsListening) return;
        NetworkManager.Singleton.StartClient();
    }

    static public void StartHostGame()
    {
        if(NetworkManager.Singleton.IsListening) return;
        NetworkManager.Singleton.StartHost();
    }
    public static void StartGameScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(GameScene, LoadSceneMode.Single);
    }
    public void StartGame()
    {
        StartGameScene();
    }
    public void QuitApplication()
    {
        Application.Quit();
    }
}
