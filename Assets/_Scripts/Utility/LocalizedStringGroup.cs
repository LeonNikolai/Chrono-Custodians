using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Chrono Custodians/LocalizedStringGroup")]
public class LocalizedStringGroup : ScriptableObject
{
    [SerializeField] LocalizedString[] _localizedStrings;
    public string GetRandomString()
    {
        if (_localizedStrings.Length == 0)
        {
            return "";
        }
        return _localizedStrings[Random.Range(0, _localizedStrings.Length)].GetLocalizedString();
    }
    public string GetString(int index)
    {
        if (index < 0 || index >= _localizedStrings.Length)
        {
            return "";
        }
        return _localizedStrings[index].GetLocalizedString();
    }
    public int Length => _localizedStrings.Length;
}