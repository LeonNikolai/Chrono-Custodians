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

    private int itemsRemaining;
    private int ItemsRemaining
    {
        get => itemsRemaining;
        set
        {
            bool changed = itemsRemaining != value;
            itemsRemaining = value;
            if (itemsRemaining < 0) itemsRemaining = 0;
            if (changed)
            {
                if (itemsRemaining == 0)
                {
                    OnLastItemSent.Invoke();
                }
            }
        }
    }
    [SerializeField] private float temporalInstability = 0;
    [SerializeField] NetworkVariable<float> TemporalInstabilityNetworked = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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
            temporalInstability += Time.deltaTime * temporalInstabilityVelocity;

            // Ratelimit the instabilityupdate
            if (Mathf.Abs(TemporalInstabilityNetworked.Value - temporalInstability) > NetworkSensitivity)
            {
                Debug.Log("Updating Temporal Instability");
                TemporalInstabilityNetworked.Value = temporalInstability;
            }
            if (temporalInstability >= temporalInstabilityMax && curLossCountdown > 0)
            {
                curLossCountdown -= Time.deltaTime;
            }

            if (curLossCountdown != lossCountdown && temporalInstability < temporalInstabilityMax)
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
            float timing = temporalInstability - TemporalInstabilityNetworked.Value;
            // Forcefull Time correction
            if (Math.Abs(timing) < NetworkSensitivity)
            {
                float sign = Mathf.Sign(timing);
                // -1 if the client is needs to decrease the value
                // 1 if the client needs to increase the value
                if (sign == 1)
                {
                    // The client is behind
                    temporalInstability = TemporalInstabilityNetworked.Value - NetworkSensitivity;
                }
                else if (sign == -1)
                {
                    // The client is ahead
                    temporalInstability = TemporalInstabilityNetworked.Value + NetworkSensitivity;
                }
            }
            temporalInstability = Mathf.MoveTowards(temporalInstability, TemporalInstabilityNetworked.Value, Time.deltaTime * temporalInstabilityVelocity);
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        slider.value = temporalInstability;
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
        if (temporalInstability < 50)
        {
            percentageText.text += $"<color=#00FF00>";
        }
        else if (temporalInstability >= 50 && temporalInstability < 75)
        {
            percentageText.text += $"<color=#FFFF00>";
        }
        else if (temporalInstability >= 75 && temporalInstability < 100)
        {
            percentageText.text += $"<color=#FF0000>";
        }
        percentageText.text += $"{Mathf.Floor(temporalInstability)}%</color>";
        if (temporalInstability >= 100)
        {
            percentageText.text = $"<color=#FF0000>Warning, Time Bubble integrity too low. Leaving time period in <color=#FFFF00>{Mathf.Ceil(curLossCountdown)}</color> seconds";
        }
    }

    private void OnItemSent(ItemSendEvent item)
    {
        if (item.ItemData.instabilityCost > 0)
        {
            ItemsRemaining--;
            int yearStart = item.ItemData.AstronomicalYearStart;
            int yearEnd = item.ItemData.AstronomicalYearEnd;
            if (item.TargetYear > yearStart && item.TargetYear < yearEnd)
            {
                temporalInstability -= 25;
                if (temporalInstability < 0) temporalInstability = 0;
                return;
            }
        }
        temporalInstabilityVelocity += 0.1f;

        itemText.text = $"{ItemsRemaining} foreign items remaining in this time period";
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
        temporalInstability = 0;
        ItemsRemaining = 10;
        temporalInstabilityVelocity = baseTemporalInstabilityVelocity;
    }
}
