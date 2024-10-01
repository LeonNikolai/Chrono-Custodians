using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public class BasicScannable : MonoBehaviour, IScanable
{
    [SerializeField] private LocalizedString _scanText;
    [SerializeField] private LocalizedString _scanDescription;
    [SerializeField] private UnityEvent _onScan = new UnityEvent();

    public string ScanTitle => _scanText.GetLocalizedString();

    public string ScanResult => _scanDescription.GetLocalizedString();

    public void OnScan(Player player)
    {
        _onScan.Invoke();
    }
}