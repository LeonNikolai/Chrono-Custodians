using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSenderSelectYear : MonoBehaviour, IInteractable, IScanable
{
    [SerializeField] ItemSender _itemSender;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] CinemachineCamera _camera;

    public bool Interactible => true;

    public string ScanTitle => "Panel";

    public string ScanResult => "Select a year";

    void Awake()
    {
        if (inputField == null) inputField = GetComponent<TMP_InputField>();
        inputField.text = _itemSender.SelectedYear.Value.ToString() ?? "2000";
        inputField.onValueChanged.AddListener(OnValueChanged);
        _itemSender.SelectedYear.OnValueChanged += OnYearChanged;
        inputField.onSubmit.AddListener(OnSubmit);
    }

    private void OnSubmit(string arg0)
    {
        if (int.TryParse(arg0, out int year))
        {
            if (year != _itemSender.SelectedYear.Value)
            {
                _itemSender.SetSelectedYearServerRpc(year);
            }
        }
        else
        {
            inputField.text = _itemSender.SelectedYear.Value.ToString();
        }
        CloseMenu();
    }

    private void OnYearChanged(int previousValue, int newValue)
    {
        inputField.text = newValue.ToString();
    }

    private void OnValueChanged(string arg0)
    {
        if (int.TryParse(arg0, out int year))
        {
            if (year != _itemSender.SelectedYear.Value)
            {
                _itemSender.SetSelectedYearServerRpc(year);
            }
        }
    }

    public void Interact(Player player)
    {
        OpenMenu();
    }

    private void OpenMenu()
    {
        inputField.Select();
        Menu.ActiveMenu = Menu.MenuType.Custom;
        Menu.CustomMenuCloseAttempt.AddListener(CloseMenu);
        Hud.Hidden = true;
        _camera.enabled = true;
    }

    private void CloseMenu()
    {
        Hud.Hidden = false;
        EventSystem.current.SetSelectedGameObject(null);

        _camera.enabled = false;
        Menu.ActiveMenu = Menu.MenuType.Closed;
    }

    public void OnScan(Player player)
    {
    }
}
