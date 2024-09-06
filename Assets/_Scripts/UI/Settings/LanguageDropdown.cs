using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

// This script is used to fill a dropdown with the available languages in the LocalizationSettings, and add logic for changing the selected locale.
public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] TMP_Dropdown _dropdown;
    void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        _dropdown.onValueChanged.AddListener(DropdownValueChanged);
        AddLanguages(_dropdown);
        if(LocalizationSettings.SelectedLocale != null) SetLocale(LocalizationSettings.SelectedLocale);
    }
    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += SelectedLocaleChanged;
    }
    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= SelectedLocaleChanged;
    }

    private void SelectedLocaleChanged(Locale locale)
    {
        if (_dropdown && _dropdown.options.Count == 0) AddLanguages(_dropdown);
        SetLocale(locale);
    }

    private void SetLocale(Locale locale = null)
    {
        _dropdown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(locale);
    }

    private void AddLanguages(TMP_Dropdown dropdownElement)
    {
        dropdownElement.options.Clear();
        var Languages = LocalizationSettings.AvailableLocales.Locales;
        foreach (var language in Languages)
        {
            dropdownElement.options.Add(new TMP_Dropdown.OptionData(language.name));
        }
    }

    private void DropdownValueChanged(int arg0)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[arg0];
    }


}
