using TMPro;
using UnityEngine;

public class UiBar : MonoBehaviour
{
    [SerializeField] private GameObject _bar;
    [SerializeField] private TMP_Text _barValueTextOptional;
    [SerializeField, Range(0, 1)] private float _progress = 0;

    void Awake()
    {
        Progress = _progress;
    }
    void OnValidate()
    {
        Progress = _progress;
    }
    public float Progress
    {
        get => _progress;
        set {
            _progress = Mathf.Clamp01(value);
            _bar.transform.localScale = new Vector3(_progress, 1, 1);
            if(_barValueTextOptional) _barValueTextOptional.text = $"{Mathf.RoundToInt(_progress * 100)}%";
        }
    }
}
