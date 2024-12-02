using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
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
        //JoinButton.onClick.AddListener(TryJoinClientFromCode);
        RefreshButton.onClick.AddListener(ListLobbies);
        //LobbyCode.onValueChanged.AddListener(SetLobbyCode);
    }

    void OnEnable()
    {
        ListLobbies();
    }

    private void SetLobbyCode(string arg0)
    {
        JoinCode = arg0;
    }

    public async void ListLobbies()
    {
        if (UnityServices.Instance.State != ServicesInitializationState.Initialized || !AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("Services not initialized or not signed in, not listing lobbies");
            return;
        }
        foreach (Transform child in ListLobbyContent)
        {
            Destroy(child.gameObject);
        }
        if (LobbyService.Instance == null)
        {
            Debug.LogError("LobbyService is null");
            return;
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
    public static JoinAllocation JoinAllocation;
    public async void TryJoinClientFromCode()
    {
        var joincode = JoinCode;
        var allocaiton = await Relay.JoinRelayAsync(joincode);
        if (allocaiton == null)
        {
            Debug.LogError("Failed to join relay");
            return;
        }
        JoinAllocation = allocaiton;
        NetworkManager.Singleton.StartClient();

    }
}