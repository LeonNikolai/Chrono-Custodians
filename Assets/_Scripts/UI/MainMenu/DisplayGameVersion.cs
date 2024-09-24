using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class DisplayGameVersion : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI _versionText;

    void Awake()
    {
        if (_versionText == null)
        {
            _versionText = GetComponent<TMPro.TextMeshProUGUI>();
        }
    }
    void Start()
    {
        _versionText.text = GetVersion;
    }
    public string GetVersion => $"<b>Build Version :</b><br>{GetVersionDate}<br><b>Build Date :</b><br>{GetVersionNumber}";
    public string GetVersionDate => SplitFirst(Application.version, '-').Item1.Trim() ?? "Unknown Version";
    public string GetVersionNumber => SplitFirst(Application.version, '-').Item2.Trim() ?? "Unknown Date";

    static (string, string) SplitFirst(string input, char delimiter)
    {
        int index = input.IndexOf(delimiter);
        if (index == -1)
        {
            return (input, string.Empty);
        }
        return (input.Substring(0, index), input.Substring(index + 1));
    }


    void OnValidate()
    {
        if (_versionText != null)
        {
            _versionText.text = GetVersion;
        }
    }
}