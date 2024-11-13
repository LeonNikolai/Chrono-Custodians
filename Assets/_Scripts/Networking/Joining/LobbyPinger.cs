using System.Collections;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyPinger : MonoBehaviour
{
    static LobbyPinger instance;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    Lobby currentLobby;
    const float PingFrequency = 15f;
    private IEnumerator HeartbeatLobbyCoroutine()
    {
        while (currentLobby != null)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            yield return new WaitForSeconds(PingFrequency);
        }
    }

    public void PingNewLobby(Lobby lobby)
    {
        StopAllCoroutines();
        instance.currentLobby = lobby;
        StartCoroutine(HeartbeatLobbyCoroutine());
    }

    public static void PingLobby(Lobby lobby)
    {
        if (instance == null)
        {
            var go = new GameObject("LobbyPinger");
            instance = go.AddComponent<LobbyPinger>();
            DontDestroyOnLoad(instance);
        }
        if (instance)
        {
            instance.PingNewLobby(lobby);
        }
    }
}