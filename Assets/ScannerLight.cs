using UnityEngine;

public class ScannerLight : MonoBehaviour
{
    [SerializeField] MeshRenderer _lightRenderer;
    [SerializeField] Material[] _on;
    [SerializeField] Material[] _off;

    void Awake()
    {
        SetState(false);
    }
    bool _state;
    public void SetState(bool on)
    {
        if (_state == on) return;
        _lightRenderer.materials = on ? _on : _off;
        _state = on;
    }
}
