using UnityEngine;

public class OpenHudInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] Menu.MenuType _menuType = Menu.MenuType.PauseMenu;

    public bool Interactable => true;

    public void Interact(Player player)
    {
        Menu.ActiveMenu = _menuType;
    }
}