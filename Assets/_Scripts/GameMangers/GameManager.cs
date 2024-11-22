using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
[DefaultExecutionOrder(-100)]
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public static ItemIdProvider ID => instance.idProvider;
    public static bool IsGameActive = true;
    public ItemIdProvider idProvider;
    public static ItemIdProvider IdProvider => instance.idProvider;
    [SerializeField] LocationRenderingSettings shipRenderingSettings = null;
    [SerializeField] LocationRenderingSettings ousideShipRenderingSettings = null;
    public NetworkVariable<LocationRenderingSettingsRefference> _outsideRenderingSettings = new NetworkVariable<LocationRenderingSettingsRefference>(LocationRenderingSettingsRefference.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<LocationRenderingSettingsRefference> _insideRenderingSettings = new NetworkVariable<LocationRenderingSettingsRefference>(LocationRenderingSettingsRefference.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public static LocationRenderingSettings OutsideRendering => instance._outsideRenderingSettings.Value.Refference == null ? instance?.shipRenderingSettings : instance._outsideRenderingSettings.Value.Refference;
    public static LocationRenderingSettings InsideRendering => instance._insideRenderingSettings.Value.Refference == null ? instance?.ousideShipRenderingSettings : instance._insideRenderingSettings.Value.Refference;
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
    public NetworkVariable<int> GameId = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public string GameID => "G-" + GameManager.instance.GameId.Value.ToString();

    public static event Action RenderingUpdate = delegate { };
    public static void UpdateRendering() => RenderingUpdate?.Invoke();
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
            GameId.Value = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            LevelManager.instance.OnLevelLoaded.AddListener(OnLevelLoaded);
        }
        IsGameActive = true;
        Player.AllPlayersDead += GameLost;
        levelState.OnValueChanged += LevelStateChanged;
        timer.OnValueChanged += TimerChanged;
        DayProgression.OnValueChanged += (previousValue, newValue) =>
        {
            Menu.ActiveMenu = Menu.MenuType.LevelStability;
        };
        _outsideRenderingSettings.OnValueChanged += UpdateRenderingSettings;
        _insideRenderingSettings.OnValueChanged += UpdateRenderingSettings;
        ItemSender.OnItemSendServer.AddListener(CheckIfSentRight);

        levelStabilities[UnityEngine.Random.Range(0, levelStabilities.Length)].Stability = 60;
    }

    private void UpdateRenderingSettings(LocationRenderingSettingsRefference previousValue, LocationRenderingSettingsRefference newValue)
    {
        RenderingUpdate?.Invoke();
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
        doorAnim.SetTrigger("CloseDoor");
        int RemainingUnstableItems = Item.CalculateRemainingUnstableItemInstability(sceneEnd.TimePeriod);
        var itemSendEvets = ItemSender.SentItems.ToArray();
        ItemSender.SentItems.Clear();

        Player.RespawnAll();
        LevelEndData data = levelStabilities.ProgressStability(sceneEnd, DayProgression.Value, itemSendEvets, RemainingUnstableItems);
        data.Level = sceneEnd;
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
            doorAnim.SetTrigger("OpenDoor");
            StartCoroutine(Timer());
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
        _timeStability.OnValueChanged -= TimeLineStabilityChanged;
        levelState.OnValueChanged -= LevelStateChanged;
        timer.OnValueChanged -= TimerChanged;

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

    [SerializeField] private Animator doorAnim;

    public NetworkVariable<float> timer = new NetworkVariable<float>(480, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void TimerChanged(float previousValue, float newValue)
    {

    }

    private void CheckIfSentRight(ItemSendEvent item)
    {
        if (IdProvider.GetItemData(item.ItemID).TimePeriods[0] == IdProvider.GetPeriodData(item.TargetPeriodID) && IdProvider.GetPeriodData(item.TargetPeriodID) != LevelManager.LoadedScene.TimePeriod)
        {
            // correct!
            timer.Value += 30;
        }
        else
        {
            timer.Value -= 30;
        }
    }

    private IEnumerator Timer()
    {
        while (timer.Value > 0)
        {
            yield return null;
            timer.Value -= Time.deltaTime;

        }
        if (timer.Value <= 0)
        {
            LevelEnd(LevelManager.instance.LoadedLevel.Value);
        }
    }

}
