using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerJoin : MonoBehaviour
{
    public string JoinCode { get; set; } = "";
    public TMP_InputField LobbyCode;
    public Button JoinButton;
    public Button RefreshButton;

    [Header("UI References")]
    public Transform ListLobbyContent;

    [Header("Prefabs")]
    public GameObject ListLobbyItemPrefab;

    void Awake()
    {
        JoinButton.onClick.AddListener(TryJoinClientFromCode);
        RefreshButton.onClick.AddListener(ListLobbies);
        LobbyCode.onValueChanged.AddListener(SetLobbyCode);
    }

    void Start()
    {
        ListLobbies();
    }

    private void SetLobbyCode(string arg0)
    {
        JoinCode = arg0;
    }

    public async void ListLobbies()
    {

        foreach (Transform child in ListLobbyContent)
        {
            Destroy(child.gameObject);
        }
        QueryResponse lobbyQuery = await LobbyService.Instance.QueryLobbiesAsync();
        foreach (var lobby in lobbyQuery.Results)
        {
            var lobbyUI = Instantiate(ListLobbyItemPrefab, ListLobbyContent);
            if (lobbyUI.TryGetComponent(out LobbyUI listItem))
            {
                listItem.Lobby = lobby;
            }
        }
    }

    public async void TryJoinClientFromCode()
    {
        var joincode = JoinCode;
        var allocaiton = await Relay.JoinRelayAsync(joincode);
        if (allocaiton == null)
        {
            Debug.LogError("Failed to join relay");
            return;
        }
        NetworkManager.Singleton.StartClient();

    }
}