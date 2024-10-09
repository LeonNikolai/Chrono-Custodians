using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemTracker : NetworkBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private TMP_Text velocityText;
    [SerializeField] private TMP_Text percentageText;


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

    private void OnItemSent(ItemSendEvent item)
    {
        if (item.ItemData.instabilityCost > 0)
        {
            ItemCount.Value--;
            itemText.text = $"{ItemCount.Value} foreign items remaining in this time period";
            int yearStart = item.ItemData.AstronomicalYearStart;
            int yearEnd = item.ItemData.AstronomicalYearEnd;
            if (item.TargetYear >= yearStart && item.TargetYear <= yearEnd)
            {
                TemporalInstabilityNetworked.Value -= 15;
                temporalInstabilityVelocity -= 0.1f;
                if (temporalInstabilityVelocity < baseTemporalInstabilityVelocity) temporalInstabilityVelocity = baseTemporalInstabilityVelocity;
                if (TemporalInstabilityNetworked.Value < 0) TemporalInstabilityNetworked.Value = 0;
            }
        }
        else
        {
            TemporalInstabilityNetworked.Value += 10;
            temporalInstabilityVelocity += 0.1f;
        }

        if (ItemCount.Value <= 0)
        {
            GameManager.instance.GameWon();
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
        TemporalInstabilityNetworked.Value = 0;
        if (IsServer) ItemCount.Value = 10;
        temporalInstabilityVelocity = baseTemporalInstabilityVelocity;
    }
}
