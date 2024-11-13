using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerHost : MonoBehaviour
{
    public Button StartHostButton;
    public TMP_InputField LobbyNameInput;

    private void Start()
    {
        StartHostButton.onClick.AddListener(StartHost);
        LobbyNameInput.onValueChanged.AddListener(SetLobbyName);
    }
    bool isLoading = false;
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            isLoading = value;
            StartHostButton.interactable = !value;
            LobbyNameInput.interactable = !value;
        }
    }

    public void SetLobbyName(string name)
    {
        LobbyName = name;
    }
    public static string JoinCode = "JoinCode";
    public static string LobbyName = "LobbyName";
    public static Lobby Lobby;

    public static Allocation HostAllocation;
    public async void StartHost()
    {
        Debug.Log("Starting Host");
        // Prevent multiple clicks
        if (IsLoading) return;
        IsLoading = true;
        JoinCode = "";
        
        try
        {
            Debug.Log("Starting Relay");
            var result = await Relay.StartHostAsync();
            HostAllocation = result.allocation;
            JoinCode = result.joinCode;
            Debug.Log("Started Relay : " + JoinCode);
            if (JoinCode != null && JoinCode.Length > 0)
            {
                var lobby = await Matchmaking.HostLobby(JoinCode, 4, LobbyName);
                Lobby = lobby;
                Debug.Log("Hosted Lobby : " + lobby.Name);
                LobbyPinger.PingLobby(lobby);
                NetworkManager.Singleton.StartHost();
                MainMenu.StartGameScene();
            }
            IsLoading = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error Starting Host : " + e.Message);
            IsLoading = false;
        }
    }
}