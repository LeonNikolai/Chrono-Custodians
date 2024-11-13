using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] Button JoinButton;
    [SerializeField] TMP_Text lobbyName;
    [SerializeField] TMP_Text playerCount;

    void Awake()
    {
        JoinButton.onClick.AddListener(() =>
        {
            lobby.JoinAsClient();
        });
    }

    void OnEnable()
    {
        Refresh();
    }
    void Start()
    {
        Refresh();
    }

    void Refresh()
    {

        if (lobby == null)
        {
            lobbyName.text = "Error";
            playerCount.text = "0";
            return;
        }
        lobbyName.text = lobby.Name;
        playerCount.text = lobby.Players.Count.ToString();
    }
    public Lobby lobby { get; private set; }
    public Lobby Lobby
    {
        get
        {
            return lobby;
        }
        set
        {
            lobby = value;
            Refresh();
        }
    }
}