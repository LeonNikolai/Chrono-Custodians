using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;


    public UnityEvent OnGameStart;
    public UnityEvent OnGameWin;
    public UnityEvent OnGameLost;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (instance == null)
        {
            instance = this;
        }else if (instance != this)
        {
            Destroy(this);

        }
    }


    public void GameLost()
    {
        // Lose logic
        OnGameLost.Invoke();
    }


    public void GameWon()
    {
        // Win logic
        OnGameWin.Invoke();
    }
}
