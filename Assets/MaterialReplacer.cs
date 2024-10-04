using UnityEngine;

public class MaterialReplacer : MonoBehaviour
{
    [SerializeField] Material _material;
    [SerializeField] private Renderer _renderer;

    Material[] _applyMaterials;
    Material[] _originalMaterials;

    void Awake()
    {
        _material = _renderer.material;
        _originalMaterials = _renderer.materials;
        _applyMaterials = new Material[_originalMaterials.Length];
        for (int i = 0; i < _originalMaterials.Length; i++)
        {
            _applyMaterials[i] = new Material(_material);
        }
        Show = false;
    }
    bool show;
    public bool Show {
        get => show;
        set {
            _renderer.materials = value ? _applyMaterials : _originalMaterials;
        }
    }
}
