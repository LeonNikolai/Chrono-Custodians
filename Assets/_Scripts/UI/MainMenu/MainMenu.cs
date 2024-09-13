using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string GameScene = "GamePlay";


    public void StartClientGame()
    {
        NetworkManager.Singleton.StartClient();
    }

    static public void StartHostGame()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(GameScene, LoadSceneMode.Single);
    }


}
