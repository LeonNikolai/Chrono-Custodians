using System;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Chrono Custodians/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] string _id;
    [Header("Tooltip Info")]
    [SerializeField] LocalizedString _name;
    [SerializeField] LocalizedString _description;
    [SerializeField] Sprite _icon;
    public Sprite Icon => _icon;
    public string Id => _id != null && String.IsNullOrEmpty(_id) ? _id : name;
    public string Name => _name != null && !_name.IsEmpty ? _name.GetLocalizedString() : name ?? "Item Name";
    public string Description => _description != null && !_description.IsEmpty ? _description.GetLocalizedString() : name ?? "Item Description";

    [Header("Item Spawn")]
    [SerializeField] GameObject _prefab;

    [Header("Hisorical Info")]
    [SerializeField] bool timelessItem = false;

    [Header("Valid Years")]
    [SerializeField] int _astronomicalYearStart = 2024;
    [SerializeField] int _astronomicalYearEnd = 2024;

    [Header("Hints")]
    [SerializeField] ItemTag[] _tags = new ItemTag[0];
    public ItemTag[] Tags => _tags;
}