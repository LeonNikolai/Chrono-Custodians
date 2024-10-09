using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
[DefaultExecutionOrder(-100)]
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public static bool IsGameActive = true;
    public UnityEvent OnGameStart = new UnityEvent();

    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.InLobby, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<LevelState> levelState = new NetworkVariable<LevelState>(LevelState.Playing, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public enum GameState
    {
        InLobby,
        InLevel
    }

    public enum LevelState
    {
        Playing,
        End_Win,
        End_Lose
    }
    private void Awake() {
        if(instance == null) {
            instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
            return;
        }
        if (IsServer)
        {
            LevelManager.instance.LevelLoaded.AddListener(OnLevelLoaded);
        }
        IsGameActive = true;
        Player.AllPlayersDead += GameLost;
        levelState.OnValueChanged += (previousValue, newValue) =>
        {
            if (newValue  == LevelState.End_Lose || newValue == LevelState.End_Win)
            {
                Menu.ActiveMenu = Menu.MenuType.LevelEnd;
            }
        };
    }

    private void OnLevelLoaded(bool levelLoaded)
    {
        if (levelLoaded) gameState.Value = GameState.InLevel;
    }

    public override void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
        Player.AllPlayersDead -= GameLost;
        IsGameActive = false;
        base.OnDestroy();
    }
    public void GameLost()
    {
        // Lose logic
        if(Player.AllPlayersAreDead)
        {
            Debug.LogWarning("All players are dead");
            levelState.Value = LevelState.End_Lose;
        }
        levelState.Value = LevelState.End_Lose;
    }

    public void GameWon()
    {
        // Win logic
        Debug.LogWarning("Game Won");
        levelState.Value = LevelState.End_Win;
    }
}
