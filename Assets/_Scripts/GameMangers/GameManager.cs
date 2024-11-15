using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
[DefaultExecutionOrder(-100)]
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public static bool IsGameActive = true;
    public ItemIdProvider idProvider;
    public static ItemIdProvider IdProvider => instance.idProvider;
    public NetworkVariable<int> _timeStability = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int TimeStability
    {
        get => _timeStability.Value;
        set
        {
            if (IsServer)
            {
                _timeStability.Value = Mathf.Clamp(value, 0, 100);
            }
        }
    }
    public int TimeInStability => 100 - TimeStability;
    public NetworkVariable<int> DayProgression = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(GameState.InLobby, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<LevelState> levelState = new NetworkVariable<LevelState>(LevelState.Playing, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<LevelEndData> lastLevelEndData = new NetworkVariable<LevelEndData>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
    private void Awake()
    {
        if (instance == null)
        {
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
        _timeStability.OnValueChanged += TimeLineStabilityChanged;
        gameState.OnValueChanged += (previousValue, newValue) =>
        {

        };
        if (IsServer)
        {
            LevelManager.instance.OnLevelLoaded.AddListener(OnLevelLoaded);
        }
        IsGameActive = true;
        Player.AllPlayersDead += GameLost;
        levelState.OnValueChanged += LevelStateChanged;
        DayProgression.OnValueChanged += (previousValue, newValue) =>
        {
            Menu.ActiveMenu = Menu.MenuType.LevelStability;
        };

        levelStabilities[UnityEngine.Random.Range(0, levelStabilities.Length)].Stability = 60;
    }

    private void LevelStateChanged(LevelState previousValue, LevelState newValue)
    {
        if (newValue == LevelState.End_Lose || newValue == LevelState.End_Win)
        {
            Menu.ActiveMenu = Menu.MenuType.LevelEnd;
        }
    }

    private void TimeLineStabilityChanged(int previousValue, int newValue)
    {
        if (IsServer)
        {
            if (newValue <= 0)
            {
                GameLost();
            }
        }
    }
    [SerializeField] public LevelStability[] levelStabilities;

    // Triggeres before the actual scene is loaded

    public LevelStability GetLevelStability(LevelScene scene)
    {
        return levelStabilities.GetLevelStability(scene);
    }
    internal LevelStability GetCurrentLevelStability()
    {
        return GetLevelStability(LevelManager.LoadedScene);
    }




    public static float ActiveLevelStability;
    public void LevelStart(LevelScene sceneStart = null, int sceneIndex = 0)
    {
        LevelStability levelStability = GetLevelStability(sceneStart);
        if (levelStability == null)
        {
            Debug.LogError("Level stability not found for " + sceneStart.LevelName);
            return;
        }
        ActiveLevelStability = levelStability._stability.Value;
        // Increase visit count of level
        foreach (var item in levelStabilities)
        {
            if (item.scene == sceneStart)
            {
                item._visitCount.Value++;
                break;
            }
        }
        sceneStart.LoadScene(sceneIndex);
    }

    NetworkVariable<int> _itemSendCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public void LevelEnd(LevelScene sceneEnd)
    {
        int RemainingUnstableItems = Item.CalculateRemainingUnstableItemInstability(sceneEnd.TimePeriod);
        var itemSendEvets = ItemSender.SentItems.ToArray();
        ItemSender.SentItems.Clear();

        Player.RespawnAll();
        LevelEndData data = levelStabilities.ProgressStability(sceneEnd,DayProgression.Value,itemSendEvets,RemainingUnstableItems);
        TimeStability = levelStabilities.TotalStability();
        lastLevelEndData.Value = data;

        if (TimeStability <= 0)
        {
            GameLost();
        }
        else
        {
            DayProgression.Value++;
        }

        DestroyCurrentLevel?.Invoke();
        LevelManager.LoadLevelScene(null);
    }
    public static event Action DestroyCurrentLevel = delegate { };



    private void OnLevelLoaded(LevelScene levelLoaded)
    {
        if (levelLoaded != null)
        {
            gameState.Value = GameState.InLevel;
        }
        else
        {
            gameState.Value = GameState.InLobby;
        }
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
        if (Player.AllPlayersAreDead)
        {
            Debug.LogWarning("All players are dead");
            levelState.Value = LevelState.End_Lose;
        }
        levelState.Value = LevelState.End_Lose;
    }

    internal void NoMoreItemsInLevel()
    {
        GameLost();
    }


}
