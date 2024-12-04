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
        if (data != null && data.Level != LevelSceneRefference.None)
        {
            var builder = new System.Text.StringBuilder();
            var fromPeriod = data.Level.Refference;
            var fromPeriodName = fromPeriod.TimePeriod?.periodName ?? "Unknown";

            builder.AppendLine($"Previous Mission Result ({fromPeriodName}):");
            
            builder.AppendLine("");
            builder.AppendLine($"<color=#00FF00>Recovered {data.SendCorrectInstability} stability from sending items to correct time periods</color>");
            builder.AppendLine($"<color=#FF0000>Lost {data.SendWrongInstability} stability from sending items to wrong time periods</color>");
            builder.AppendLine($"<color=#FF0000>Lost {data.PlayersOutsideStabilityLoss} stability from players not leaving with the time-machine</color>");
            
            builder.AppendLine("");
            builder.AppendLine($"<color=#FF0000>Lost Stability From Time-Tourism : {-data.OtherDecrease}</color>");

            builder.AppendLine("");
            builder.AppendLine("From : " + data.Level.Refference?.TimePeriod?.periodName ?? "Unknown Period");
            builder.AppendLine("Items Sent :");

            var wrongItems = 0;
            var correctItems = 0;
            var awayFromPeriod = 0;
            foreach (ItemSendEvent item in data.itemSendEvets)
            {
                var targetPerioName = item.TargetPeriod?.periodName ?? "Unknown";

                var stabilityChange = item.InstabilityWorth;
                // sending items that belong
                if (item.ItemData.TimePeriods.Contains(fromPeriod.TimePeriod))
                {
                    if (item.TargetPeriod == fromPeriod.TimePeriod)
                    {
                        builder.AppendLine($"<color=#FFFF00>Sent {item.ItemData?.Name ?? "Item"} from {fromPeriodName} back to its origin {targetPerioName}</color>");
                        continue;
                    }
                    builder.AppendLine($"<color=#FF0000>Sent {item.ItemData?.Name ?? "Item"} away from {fromPeriodName} to {targetPerioName} (wrong) (-{stabilityChange})</color>");
                    awayFromPeriod++;
                    continue;
                }

                // sending items that dont belong
                if (item.ItemData.TimePeriods.Contains(item.TargetPeriod))
                {
                    builder.AppendLine($"<color=#00FF00>Sent {item.ItemData?.Name ?? "Item"} to {targetPerioName} (correct) (+{stabilityChange})</color>");
                    correctItems++;
                }
                else
                {
                    builder.AppendLine($"<color=#FF0000>Sent {item.ItemData?.Name ?? "Item"} to {targetPerioName} (wrong) (-{stabilityChange})</color>");
                    wrongItems++;
                }
            }
            builder.AppendLine("");
            builder.AppendLine("");
            builder.AppendLine($"Items sent to correct period : {correctItems}");
            builder.AppendLine($"Items sent to wrong period : {wrongItems}");
            builder.AppendLine($"Items sent away from origin period ({fromPeriodName}) : {awayFromPeriod}");
            _itemText.text = builder.ToString();
        }
        else
        {
            _itemText.text = "Finnish a level to see mission results";
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
