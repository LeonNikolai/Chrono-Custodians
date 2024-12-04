using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class TimeLineStabilityUI : MonoBehaviour
{
    [SerializeField] UiBar _stabilityBar;
    [SerializeField] TMP_Text _dayText;
    [SerializeField] TMP_Text _itemText;

    public float _previousStability;

    void Start()
    {
        if (_stabilityBar == null) _stabilityBar = GetComponent<UiBar>();
    }

    void OnEnable()
    {
        if (GameManager.instance == null) return;
        GameManager.instance._timeStability.OnValueChanged += OnStabilityChange;
        GameManager.instance.DayProgression.OnValueChanged += DayProgression;
        GameManager.instance.lastLevelEndData.OnValueChanged += RefreshText;
        OnStabilityChange(GameManager.instance._timeStability.Value, GameManager.instance._timeStability.Value);
        DayProgression(GameManager.instance.DayProgression.Value, GameManager.instance.DayProgression.Value);
        RefreshText(GameManager.instance.lastLevelEndData.Value, GameManager.instance.lastLevelEndData.Value);

    }

    private void RefreshText(LevelEndData previousValue, LevelEndData newValue)
    {
        var data = newValue;
        if (data != null)
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Data");
            builder.AppendLine($"Recovered {data.SendCorrectInstability} stability from sending items to correct time periods");
            builder.AppendLine($"Lost {data.SendWrongInstability} stability from sending items to wrong time periods");
            builder.AppendLine($"{data.RemainingUnstableItemStability} instability was left in the time period");
            builder.AppendLine("");
            builder.AppendLine("From : " + data.Level.Refference?.TimePeriod?.periodName ?? "Unknown Period");
            builder.AppendLine("Items Sent :");
            var fromPeriod = data.Level.Refference;
            if (fromPeriod == TimePeriodRefference.None)
            {
                builder.AppendLine("No target period");
                _itemText.text = builder.ToString();
                return;
            }
            foreach (ItemSendEvent item in data.itemSendEvets)
            {
                var targetPerioName = item.TargetPeriod?.periodName ?? "Unknown";
                if(item.TargetPeriod == fromPeriod) {
                    builder.AppendLine($"<color=#FFFF00>Sent {item.ItemData?.Name ?? "Item"} back to {targetPerioName}</color>");
                    continue;
                }
                if (item.ItemData.TimePeriods.Contains(item.TargetPeriod))
                {
                    builder.AppendLine($"<color=#00FF00>Sent {item.ItemData?.Name ?? "Item"} to {targetPerioName} (correct)</color>");
                }
                else
                {
                    builder.AppendLine($"<color=#FF0000>Sent {item.ItemData?.Name ?? "Item"} to {targetPerioName} (wrong)</color>");
                }
            }
            _itemText.text = builder.ToString();
        }
        else
        {
            _itemText.text = "No data";
        }
    }

    private IEnumerator UpdateStability(int previousValue, int newValue)
    {
        float targetValue = newValue;
        float previous = _previousStability;
        _previousStability = newValue;
        float lerp = 0;
        while (lerp < 1)
        {
            lerp += Time.deltaTime;
            _stabilityBar.Progress = Mathf.Lerp(previous, targetValue, lerp) / 100f;
            yield return null;
        }
        _stabilityBar.Progress = newValue / 100f;
    }

    void OnDisable()
    {
        if (GameManager.instance)
        {
            GameManager.instance._timeStability.OnValueChanged -= OnStabilityChange;
            GameManager.instance.DayProgression.OnValueChanged -= DayProgression;
        }
    }

    private void DayProgression(int previousValue, int newValue)
    {
        if (_dayText) _dayText.text = $"Day {newValue}";
    }

    void OnDestroy()
    {

    }

    private void OnStabilityChange(int previousValue, int newValue)
    {
        if (_previousStability == newValue) return;
        StartCoroutine(UpdateStability(previousValue, newValue));
    }
}
