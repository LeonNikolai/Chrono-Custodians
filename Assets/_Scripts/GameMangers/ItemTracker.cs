using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemTracker : NetworkBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private TMP_Text velocityText;
    [SerializeField] private TMP_Text percentageText;

    [SerializeField] private int itemsRemaining;
    [SerializeField] private float temporalInstability = 0;
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
    }

    private void Update()
    {
        if (hasStarted)
        {
            temporalInstability += Time.deltaTime * temporalInstabilityVelocity;
            slider.value = temporalInstability;
            if (itemsRemaining == 0)
            {
                OnLastItemSent.Invoke();
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
                OnInstabilityThresholdReached.Invoke();
            }
        }

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
        }else if (temporalInstability >= 75 && temporalInstability < 100)
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
            itemsRemaining--;
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

        itemText.text = $"{itemsRemaining} foreign items remaining in this time period";
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
        itemsRemaining = 0;
        temporalInstabilityVelocity = baseTemporalInstabilityVelocity;
    }
}
