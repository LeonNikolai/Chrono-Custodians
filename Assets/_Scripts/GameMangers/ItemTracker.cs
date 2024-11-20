using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemTracker : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private ItemIdProvider _itemIdProvider;
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private TMP_Text velocityText;
    [SerializeField] private TMP_Text percentageText;

    [Header("Settings")]
    [SerializeField] NetworkVariable<float> TemporalInstabilityNetworked = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> ItemCount = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private float baseTemporalInstabilityVelocity = 0.2f;
    private float temporalInstabilityVelocity = 0.5f;
    [SerializeField] private float temporalInstabilityMax = 100;

    [SerializeField] private bool hasStarted = false;

    private float lossCountdown = 3;
    private float curLossCountdown;

    public UnityEvent OnLastItemSent;
    public UnityEvent OnInstabilityThresholdReached;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ItemSender.OnItemSendServer.AddListener(OnItemSent);
        GameManager.instance.gameState.OnValueChanged += OnGameStateChanged;
        if (GameManager.instance.gameState.Value == GameManager.GameState.InLevel)
        {
            StartGame();
        }
    }

    private void OnGameStateChanged(GameManager.GameState previousValue, GameManager.GameState newValue)
    {
        if (newValue == GameManager.GameState.InLevel)
        {
            StartGame();
        }
        else
        {
            RestartTimer();
        }
    }

    public override void OnDestroy()
    {
        ItemSender.OnItemSendServer.RemoveListener(OnItemSent);
        base.OnDestroy();
    }

    public bool stabilityReached = false;
    public bool StabilityReached
    {
        get => stabilityReached;
        set
        {
            if (value != stabilityReached)
            {
                stabilityReached = value;
                if (stabilityReached)
                {

                    OnInstabilityReached();
                }
            }
        }
    }

    private void OnInstabilityReached()
    {
        Debug.LogWarning("Instability Reached");
        GameManager.instance.GameLost();
        OnInstabilityThresholdReached.Invoke();
    }
    const float NetworkSensitivity = 0.04f;
    private void Update()
    {
        if (!hasStarted) return;
        if (IsServer)
        {
            TemporalInstabilityNetworked.Value += Time.deltaTime * temporalInstabilityVelocity;

            if (TemporalInstabilityNetworked.Value >= temporalInstabilityMax && curLossCountdown > 0)
            {
                curLossCountdown -= Time.deltaTime;
            }

            if (curLossCountdown != lossCountdown && TemporalInstabilityNetworked.Value < temporalInstabilityMax)
            {
                curLossCountdown = lossCountdown;
            }

            if (curLossCountdown <= 0)
            {
                curLossCountdown = 0;
                StabilityReached = true;
            }
            else
            {
                StabilityReached = false;
            }
            UpdateVisuals();
        }
        else if (IsClient)
        {
            UpdateVisuals();
        }

    }
    public float OldTemporalInstability { get; private set; }
    private void UpdateVisuals()
    {
        itemText.text = $"{ItemCount.Value} foreign items remaining in this time period";
        if (OldTemporalInstability == TemporalInstabilityNetworked.Value) return;
        slider.value = TemporalInstabilityNetworked.Value;
        velocityText.text = $"Time Bubble Degradation: ";
        if (temporalInstabilityVelocity < 0.75)
        {
            velocityText.text += $"<color=#FFFF00>";
        }
        else if (temporalInstabilityVelocity >= 0.75)
        {
            velocityText.text += $"<color=#FF0000>";
        }
        velocityText.text += $"{temporalInstabilityVelocity}/s</color>";

        percentageText.text = $"Time Bubble Integrity Loss: ";
        if (TemporalInstabilityNetworked.Value < 50)
        {
            percentageText.text += $"<color=#00FF00>";
        }
        else if (TemporalInstabilityNetworked.Value >= 50 && TemporalInstabilityNetworked.Value < 75)
        {
            percentageText.text += $"<color=#FFFF00>";
        }
        else if (TemporalInstabilityNetworked.Value >= 75 && TemporalInstabilityNetworked.Value < 100)
        {
            percentageText.text += $"<color=#FF0000>";
        }
        percentageText.text += $"{Mathf.Floor(TemporalInstabilityNetworked.Value)}%</color>";
        if (TemporalInstabilityNetworked.Value >= 100)
        {
            percentageText.text = $"<color=#FF0000>Warning, Time Bubble integrity too low. Leaving time period in <color=#FFFF00>{Mathf.Ceil(curLossCountdown)}</color> seconds";
        }
        OldTemporalInstability = TemporalInstabilityNetworked.Value;
    }

    [Rpc(SendTo.ClientsAndHost)] // Type - 0 = incorrect | 1 = correct | 2 = item should not have been sent
    public void ItemSentClientFeedbackRpc(int type, ItemDataRefference refferencedata, int periodID, float instabilityChange)
    {
        Debug.Log("Item Tracker Received");
        ItemData data = refferencedata.Refference;
        Hud.ItemSentFeedback(type, data, GameManager.instance.idProvider.GetPeriodData(periodID), instabilityChange);
    }

    private void OnItemSent(ItemSendEvent item)
    {

        itemText.text = $"{ItemCount.Value} foreign items remaining in this time period";
        bool isCorrectTimePeriod = false;
        foreach (TimePeriod period in item.ItemData.TimePeriods)
        {
            if (GameManager.instance.idProvider.GetPeriodData(item.TargetPeriodID).periodName == period.periodName)
            {
                isCorrectTimePeriod = true;
                break;
            }
        }

        if (item.ItemData.TimePeriods[0] == LevelManager.LoadedScene.TimePeriod)
        {
            TemporalInstabilityNetworked.Value += 10;
            temporalInstabilityVelocity += 0.1f;
            ItemSentClientFeedbackRpc(2, item.ItemData.NetworkedRefference, item.TargetPeriodID, 10);
            return;
        }

        if (isCorrectTimePeriod)
        {
            ItemCount.Value--;
            TemporalInstabilityNetworked.Value -= 15;
            temporalInstabilityVelocity -= 0.1f;
            if (temporalInstabilityVelocity < baseTemporalInstabilityVelocity) temporalInstabilityVelocity = baseTemporalInstabilityVelocity;
            if (TemporalInstabilityNetworked.Value < 0) TemporalInstabilityNetworked.Value = 0;
            ItemSentClientFeedbackRpc(1, item.ItemData.NetworkedRefference, item.TargetPeriodID, -15);
        }
        else
        {
            TemporalInstabilityNetworked.Value += 10;
            temporalInstabilityVelocity += 0.1f;
            ItemSentClientFeedbackRpc(0, item.ItemData.NetworkedRefference, item.TargetPeriodID, 10);
        }


        if (ItemCount.Value <= 0)
        {
            GameManager.instance.NoMoreItemsInLevel();
        }
    }

    public void StartTimer(bool enabled)
    {
        hasStarted = enabled;
    }

    public void StartGame()
    {
        RestartTimer();
        StartTimer(true);
    }

    public void RestartTimer()
    {
        hasStarted = false;
        if (IsServer)
        {
            TemporalInstabilityNetworked.Value = 0;
            ItemCount.Value = 10;
        }
        temporalInstabilityVelocity = baseTemporalInstabilityVelocity;
    }
}
