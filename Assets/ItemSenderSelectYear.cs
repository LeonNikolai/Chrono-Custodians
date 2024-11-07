using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSenderSelectYear : MonoBehaviour, IInteractable, IScanable
{
    [SerializeField] ItemSender _itemSender;
    [SerializeField] CinemachineCamera _camera;

    public bool Interactable => true;

    public string ScanTitle => "Panel";

    public string ScanResult => "Select a year";

    void Awake()
    {
    }

    public void Interact(Player player)
    {
        OpenMenu();
    }

    private void OpenMenu()
    {
        Menu.ActiveMenu = Menu.MenuType.Custom;
        Menu.CustomMenuCloseAttempt.AddListener(CloseMenu);
        _camera.enabled = true;
    }

    private void CloseMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);

        _camera.enabled = false;
        Menu.ActiveMenu = Menu.MenuType.Closed;
    }

    public void OnScan(Player player)
    {
    }
}
