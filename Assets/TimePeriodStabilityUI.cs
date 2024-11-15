using System.Collections;
using TMPro;
using UnityEngine;

public class TimePeriodStabilityUI : MonoBehaviour
{
    [SerializeField] LevelScene scene;
    [Header("UI Refferences")]
    [SerializeField] UiBar _stabilityBar;
    [SerializeField] TMP_Text _stabilityText;
    LevelStability _levelStability;
    float _previousStability = 100;


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

    void OnDestroy()
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
        _stabilityText.text = scene.LevelName;
        if (_previousStability == newValue) return;
        StartCoroutine(UpdateStability(previousValue, newValue));
    }
    private IEnumerator UpdateStability(float previousValue, float newValue)
    {
        float targetValue = newValue;
        float previous = _previousStability;
        _previousStability = newValue;
        float lerp = 0;
        while (lerp < 1)
        {
            lerp += Time.deltaTime * 1;
            _stabilityBar.Progress = Mathf.Lerp(previous, targetValue, lerp) / 100f;
            yield return null;
        }

        float scale = 1;
        while (scale < 1.1f)
        {
            scale = Mathf.MoveTowards(scale, 1.1f, Time.deltaTime * 3);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        while (scale > 1)
        {
            scale = Mathf.MoveTowards(scale, 1, Time.deltaTime * 2);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        _stabilityBar.Progress = targetValue / 100;
    }
}
