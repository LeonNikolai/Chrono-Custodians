using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
/// <summary>
/// This class is responsible for the Menu of the game.
/// </summary>
public class Menu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _menuRoot;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _levelSelection;
    public static UnityEvent CustomMenuCloseAttempt;
    public enum MenuType
    {
        Closed,
        PauseMenu,
        LevelSelection,
        Custom
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
            case MenuType.Custom:
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
        if(CustomMenuCloseAttempt == null) CustomMenuCloseAttempt = new UnityEvent();
        if (instance == null) instance = this;
        else Destroy(gameObject);
        RefreshMenu();
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
                return;
            }
            if (currentMenu == MenuType.Custom)
            {
                CustomMenuCloseAttempt.Invoke();
                return;
            }

            ActiveMenu = MenuType.Closed;

        }
    }


    public void QuitGame()
    {
        Application.Quit();
    }
    public void GoToMainMenu()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }
}