using UnityEngine;

using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    [Header("Tooltip Info")]
    [SerializeField] LocalizedString _name;
    [SerializeField] LocalizedString _description;
    [SerializeField] LocalizedString _icon;
    

    [Header("Item Spawn")]
    [SerializeField] GameObject _prefab;


    [Header("Hisorical Info")]
    [SerializeField] int _astronomicalYear = 2024;
    [SerializeField] bool timelessItem = false;
}