using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using System.Collections;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

public static class Matchmaking
{
    private static Lobby currentLobby;
    public static string JoinCode(this Lobby lobby)
    {
        return lobby.Data["joinCode"].Value;
    }
    public static async void JoinAsClient(this Lobby lobby)
    {
        var joinCode = lobby.JoinCode();
        var allocaiton = await Relay.JoinRelayAsync(joinCode);
        MultiplayerJoin.JoinAllocation = allocaiton;
        if (allocaiton == null)
        {
            Debug.LogError("Failed to join relay");
            return;
        }
        NetworkManager.Singleton.StartClient();

    }
    public static async Task<Lobby> HostLobby(string JoinCode, int maxPlayers = 4, string name = "Lobby")
    {
        try
        {
            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, JoinCode) }
                }
            };
            var lobby = await LobbyService.Instance.CreateLobbyAsync(name, maxPlayers, options);
            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobby creation error: {e.Message}");
        }
        return null;
    }
}