using UnityEngine;

public class OpenHudInteraction : MonoBehaviour, IInteractable, IHighlightable
{
    [SerializeField] Menu.MenuType _menuType = Menu.MenuType.PauseMenu;
    public void HightlightEnter()
    {
    }

    public void HightlightExit()
    {
    }

    public void HightlightUpdate()
    {
    }

    public void Interact(PlayerMovement player)
    {
        Menu.ActiveMenu = _menuType;
    }
}