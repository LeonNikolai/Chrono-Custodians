using System;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Chrono Custodians/ItemTags")]
public class ItemTag : ScriptableObject
{
    [SerializeField] string _id;
    [Header("Tag Tooltip Info")]
    [SerializeField] LocalizedString _displayName;
    [SerializeField] LocalizedString _description;
    public string Name => _displayName.GetLocalizedString();
    public string Description => _description.GetLocalizedString();
    public string Id => _id != null && String.IsNullOrEmpty(_id) ? _id : name;
}