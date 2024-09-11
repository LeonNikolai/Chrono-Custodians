using UnityEngine;

public class UiBar : MonoBehaviour
{
    [SerializeField] private GameObject _bar;
    public float Progress
    {
        set => _bar.transform.localScale = new Vector3(value, 1, 1);
        get => _bar.transform.localScale.x;
    }
}
