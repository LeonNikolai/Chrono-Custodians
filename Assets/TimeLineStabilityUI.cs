using System;
using System.Collections;
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
        OnStabilityChange(GameManager.instance._timeStability.Value, GameManager.instance._timeStability.Value);
        DayProgression(GameManager.instance.DayProgression.Value, GameManager.instance.DayProgression.Value);

        var data = GameManager.instance.lastLevelEndData.Value;
        if(data != null) {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Data");
            builder.AppendLine($"Sent {data.SendCorrectInstability} items to correct time periods");
            builder.AppendLine($"Sent {data.SendWrongInstability} items to wrong time periods");
            builder.AppendLine($"{data.RemainingUnstableItemStability} unstable items was left in the time period");
            builder.AppendLine("");
            builder.AppendLine("Items :");
            foreach (var item in data.itemSendEvets)
            {
                if(item.LevelId == data.LevelId) {
                    builder.AppendLine($"<color=#FFFFFF>Sent {item.ItemData.Name} to {item.Level.LevelName}</color>");
                } else {
                    builder.AppendLine($"<color=#FF0000>Sent {item.ItemData.Name} to {item.Level.LevelName}</color>");
                }
            }
            _itemText.text = builder.ToString();
        } else {
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
        if(_previousStability == newValue) return;
        StartCoroutine(UpdateStability(previousValue, newValue));
    }
}
