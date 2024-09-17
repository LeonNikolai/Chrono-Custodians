using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    [Header("Tooltip Info")]
    [SerializeField] LocalizedString _name;
    [SerializeField] LocalizedString _description;
    [SerializeField] Sprite _icon;
    public Sprite Icon => _icon;
    

    [Header("Item Spawn")]
    [SerializeField] GameObject _prefab;


    [Header("Hisorical Info")]
    [SerializeField] bool timelessItem = false;
    [Header("Valid Years")]
    [SerializeField] int _astronomicalYearStart = 2024;
    [SerializeField] int _astronomicalYearEnd = 2024;
}