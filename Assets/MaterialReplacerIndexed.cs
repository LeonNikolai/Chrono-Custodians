using System;
using System.Collections.Generic;
using UnityEngine;

public class MaterialReplacerIndexed : MonoBehaviour
{
    [Serializable]
    struct MaterialArray
    {
        public Material[] materials;
    }
    [SerializeField] MaterialArray[] _materialSelections;
    [SerializeField] Renderer _renderer;
    Material[][] _materials;

    void Awake()
    {
        var mats = new List<Material[]>();
        mats.Add(_renderer.materials);
        foreach (var item in _materialSelections)
        {
            mats.Add(item.materials);
        }
        _materials = mats.ToArray();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        Index = 0;
    }

    int index = -1;
    public int Index
    {
        get => index;
        set
        {
            index = value;
            if(index < 0 || index >= _materials.Length) return;
            _renderer.materials = _materials[value % _materials.Length];
        }
    }
}
