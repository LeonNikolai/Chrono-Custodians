using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string GameScene = "GamePlay";


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
    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(GameScene, LoadSceneMode.Single);
    }


}
