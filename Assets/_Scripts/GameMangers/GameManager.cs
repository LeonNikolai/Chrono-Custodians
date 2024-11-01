using System;
using Unity.Netcode;
using UnityEngine;
[DefaultExecutionOrder(-100)]
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public static bool IsGameActive = true;
    public ItemIdProvider idProvider;
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
            LevelManager.instance.LevelLoaded.AddListener(OnLevelLoaded);
        }
        IsGameActive = true;
        Player.AllPlayersDead += GameLost;
        levelState.OnValueChanged += LevelStateChanged;
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
    [SerializeField] public LevelStability[] _levelStabilities;

    // Triggeres before the actual scene is loaded
    public void LevelStart(LevelScene sceneStart = null)
    {
        // Increase visit count of level
        foreach (var item in _levelStabilities)
        {
            if (item.scene == sceneStart)
            {
                item._visitCount.Value++;
                break;
            }
        }

    }

    public void LevelEnd(LevelScene sceneEnd)
    {
        RespawnAllPlayers();
        ProgressStability(sceneEnd);
        if (TimeStability <= 0)
        {
            GameLost();
        }
        else
        {
            DayProgression.Value++;
        }
    }

    private void RespawnAllPlayers()
    {
        foreach (var player in Player.AllPlayers)
        {
            player.Movement.TeleportToSpawnRpc();
        }
    }

    private void ProgressStability(LevelScene completedScene)
    {
        int day = DayProgression.Value;

        float rand1 = Mathf.Sin(day) * Mathf.Exp(day * 0.1f);
        float rand2 = 3 + Mathf.Sin(day * 3) * Mathf.Exp(day * 0.15f);
        int instabilityToDistribute = day + Mathf.FloorToInt(UnityEngine.Random.Range(rand1, rand2));
        if (_levelStabilities == null || _levelStabilities.Length == 0)
        {
            return;
        }
        // Distribute some random instability to all levels
        while (instabilityToDistribute > 0)
        {
            int randomLevel = UnityEngine.Random.Range(0, _levelStabilities.Length);
            LevelStability scene = _levelStabilities[randomLevel];
            if (scene.scene == completedScene) continue;

            // reduce the amount of instability to distribute to levels with more visits
            int max = instabilityToDistribute - Mathf.FloorToInt(scene.Visits);

            // Random amount of instability to distribute
            int amount = UnityEngine.Random.Range(1, Math.Max(max, 1));

            // Distribute the instability
            instabilityToDistribute -= randomLevel;
            scene.Stability -= amount;
        }

        // Flat instability scaling with day for one level
        var randomWithMoreIncrese = _levelStabilities[UnityEngine.Random.Range(0, _levelStabilities.Length)];
        randomWithMoreIncrese.Stability -= 1 + day - randomWithMoreIncrese.Visits;

        // Flat, Exponential instability scaling with day for all levels
        int dayClamped = Mathf.Max(DayProgression.Value - 2, 0);
        float exponentiaDecrese = Mathf.Exp(dayClamped / 5) - Mathf.Exp(dayClamped / 10);
        foreach (var levelStability in _levelStabilities)
        {
            // If the level was just completed, reset it's stability if
            if (completedScene == levelStability.scene)
            {
                levelStability.Stability = 100;
                continue;
            }
            levelStability.Stability -= exponentiaDecrese;
        }

        int count = _levelStabilities.Length;
        float totalStability = 0;
        foreach (var levelStability in _levelStabilities)
        {
            totalStability += levelStability.Stability;
        }
        totalStability /= count;
        TimeStability = Mathf.FloorToInt(totalStability);
    }

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
