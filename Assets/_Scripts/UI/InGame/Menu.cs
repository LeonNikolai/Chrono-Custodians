using System;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// This class is responsible for the Menu of the game.
/// </summary>
public class Menu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _levelSelection;

    public enum MenuType
    {
        Closed,
        PauseMenu,
        LevelSelection
    }

    public static MenuType currentMenu = MenuType.Closed;
    public static MenuType ActiveMenu
    {
        get => currentMenu;
        set
        {
            currentMenu = value;
            instance?.RefreshMenu();
        }
    }

    public void RefreshMenu()
    {
        DeactivateAllMenus();
        if (currentMenu == MenuType.Closed)
        {
            _menuRoot.SetActive(false);
            Player.InMenu = false;
            return;
        }
        _menuRoot.SetActive(true);
        Player.InMenu = true;
        switch (currentMenu)
        {
            case MenuType.Closed:
                break;
            case MenuType.PauseMenu:
                _pauseMenu?.SetActive(true);
                break;
            case MenuType.LevelSelection:
                _levelSelection?.SetActive(true);
                break;
        }
    }

    private void DeactivateAllMenus()
    {
        _menuRoot.SetActive(false);
        _pauseMenu.SetActive(false);
        _levelSelection.SetActive(false);
    }

    static public Menu instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentMenu == MenuType.Closed)
            {
                ActiveMenu = MenuType.PauseMenu;
            }
            else
            {
                ActiveMenu = MenuType.Closed;
            }
        }
    }
}