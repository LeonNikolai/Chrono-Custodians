using TMPro;
using UnityEngine;

public class TimePeriodStabilityUI : MonoBehaviour
{
    [SerializeField] LevelScene scene;
    [Header("UI Refferences")]
    [SerializeField] UiBar _stabilityBar;
    [SerializeField] TMP_Text _stabilityText;
    LevelStability _levelStability;
    void Start()
    {
        SetLevel(scene);
        OnStabilityChange(_levelStability._stability.Value, _levelStability._stability.Value);
    }
    private void OnValidate()
    {
        if (_stabilityText != null && scene != null) _stabilityText.text = scene.LevelName;
    }

    private void OnEnable()
    {
        if (_levelStability != null)
        {
            _levelStability._stability.OnValueChanged += OnStabilityChange;
            OnStabilityChange(_levelStability._stability.Value, _levelStability._stability.Value);
        }
    }
    private void OnDisable()
    {
        if (_levelStability != null)
        {
            _levelStability._stability.OnValueChanged -= OnStabilityChange;
        }
    }


    private void SetLevel(LevelScene scene)
    {
        this.scene = scene;
        if (_levelStability != null)
        {
            _levelStability._stability.OnValueChanged -= OnStabilityChange;
        }
        _levelStability = Stability.GetLevelStability(scene);
        _levelStability._stability.OnValueChanged += OnStabilityChange;
    }


    private void OnStabilityChange(float previousValue, float newValue)
    {
        _stabilityBar.Progress = newValue / 100;
        _stabilityText.text = scene.LevelName;
    }
}
