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
    [SerializeField] private GameObject _levelEndScreen;
    [SerializeField] private GameObject _levelInstabilityScreen;
    public static UnityEvent CustomMenuCloseAttempt;
    public enum MenuType
    {
        Closed,
        PauseMenu,
        LevelSelection,
        LevelEnd,
        LevelStability,
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
            case MenuType.LevelEnd:
                _levelEndScreen?.SetActive(true);
                break;
            case MenuType.LevelStability:
                _levelInstabilityScreen?.SetActive(true);
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
        _levelEndScreen.SetActive(false);
        _levelInstabilityScreen.SetActive(false);
    }

    static public Menu instance;

    private void Awake()
    {
        if (CustomMenuCloseAttempt == null) CustomMenuCloseAttempt = new UnityEvent();
        if (instance == null) instance = this;
        else Destroy(gameObject);
        RefreshMenu();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void CloseAttempt()
    {
        if (currentMenu == MenuType.Custom)
        {
            CustomMenuCloseAttempt.Invoke();
            return;
        }
        if (currentMenu == MenuType.LevelEnd) return;

        ActiveMenu = MenuType.Closed;
    }
    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (currentMenu == MenuType.Closed)
            {
                ActiveMenu = MenuType.LevelStability;
                return;
            }
            if (currentMenu == MenuType.LevelStability)
            {
                CloseAttempt();
                return;
            }
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentMenu == MenuType.Closed)
            {
                ActiveMenu = MenuType.PauseMenu;
                return;
            }
            CloseAttempt();
        }
    }


    public void QuitGame()
    {
        Application.Quit();
    }
    public void GoToMainMenu()
    {
        Destroy(NetworkManager.Singleton);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }

    internal static void Close()
    {
        instance.CloseAttempt();
    }
}