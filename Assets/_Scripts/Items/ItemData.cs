using System;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Chrono Custodians/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] string _id;
    [Header("Tooltip Info")]
    [SerializeField] LocalizedString _name;
    [SerializeField] LocalizedString[] _scanMinigameResults;
    [SerializeField] LocalizedString _description;
    [SerializeField] Sprite _icon;
    public Sprite Icon => _icon;
    public int Id => GameManager.IdProvider.GetItemId(this);
    /*
    public int AstronomicalYearStart => _astronomicalYearStart;
    public int AstronomicalYearEnd => _astronomicalYearEnd;*/
    public TimePeriod[] TimePeriods => _timePeriods;
    public string Name => _name != null && !_name.IsEmpty ? _name.GetLocalizedString() : name ?? "Item Name";
    public string Description => _description != null && !_description.IsEmpty ? _description.GetLocalizedString() : name ?? "Item Description";
    public LocalizedString[] ScanMinigameResults => _scanMinigameResults;

    [Header("Item Spawn")]
    [SerializeField] GameObject _prefab;
    public GameObject Prefab => _prefab;

    [Header("Hisorical Info")]
    [SerializeField] bool _unSendable = false;
    public bool UnSendable => _unSendable;

    /*
    [Header("Valid Years")]
    [SerializeField] int _astronomicalYearStart = 2024;
    [SerializeField] int _astronomicalYearEnd = 2024;
    */
    [Header("Time Period")]
    [SerializeField] TimePeriod[] _timePeriods;

    [Header("Hints")]
    [SerializeField] ItemTag[] _tags = new ItemTag[0];
    public ItemTag[] Tags => _tags;

    public ItemDataRefference NetworkedRefference => new ItemDataRefference(this);
}