using System;
using TMPro;
using UnityEngine;

public class TimeLineStabilityUI : MonoBehaviour
{
    [SerializeField] UiBar _stabilityBar;
    [SerializeField] TMP_Text _dayText;
    void Start()
    {
        if (_stabilityBar == null) _stabilityBar = GetComponent<UiBar>();
        GameManager.instance._timeStability.OnValueChanged += OnStabilityChange;
        GameManager.instance.DayProgression.OnValueChanged += DayProgression;
        OnStabilityChange(GameManager.instance._timeStability.Value, GameManager.instance._timeStability.Value);
        DayProgression(GameManager.instance.DayProgression.Value, GameManager.instance.DayProgression.Value);
    }

    private void DayProgression(int previousValue, int newValue)
    {
        if (_dayText) _dayText.text = $"Day {newValue}";
    }

    void OnDestroy()
    {
        if (GameManager.instance)
        {
            GameManager.instance._timeStability.OnValueChanged -= OnStabilityChange;
            GameManager.instance.DayProgression.OnValueChanged -= DayProgression;
        }
    }

    private void OnStabilityChange(int previousValue, int newValue)
    {
        if (_stabilityBar) _stabilityBar.Progress = newValue / 100f;
    }
}
