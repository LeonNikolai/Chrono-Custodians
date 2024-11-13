using System;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UIElements;

public class VivoxManager : NetworkBehaviour
{
    // Join a Vivox voice channel
    public NetworkVariable<FixedString64Bytes> GameChannelName = new NetworkVariable<FixedString64Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public string channelName = "";
    public string channels = "";
    public string activeInput = "";
    public string activeOutput = "";
    public override async void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        VivoxService.Instance.LoggedIn += onLoggedIn;
        VivoxService.Instance.LoggedOut += onLoggedOut;
        VivoxService.Instance.ChannelJoined += onChannelJoined;
        VivoxService.Instance.ChannelLeft += onChannelLeft;
        VivoxService.Instance.ParticipantAddedToChannel += AddedParticipant;
        VivoxService.Instance.ParticipantRemovedFromChannel += RemovedParticipant;
        if (IsHost)
        {
            GameChannelName.Value = MultiplayerHost.Lobby.Id;
        }
        await Init();
    }

    private void RemovedParticipant(VivoxParticipant participant)
    {
        Debug.Log("Removed participant : " + participant.DisplayName);
    }

    private void AddedParticipant(VivoxParticipant participant)
    {
        Debug.Log("Added participant : " + participant.DisplayName);
    }

    private void onChannelJoined(string obj)
    {
        Debug.Log("Joined channel : " + obj);
        inChannel = true;
    }

    private void onChannelLeft(string obj)
    {
        Debug.Log("Left channel : " + obj);
        inChannel = false;
    }

    private void onLoggedOut()
    {
        Debug.Log("Logged out of Vivox");
        loggedIn = false;
    }

    private void onLoggedIn()
    {
        Debug.Log("Logged in to Vivox");
        loggedIn = true;
    }

    public bool loggedIn = false;
    public bool inChannel = false;

    public async Task Init()
    {
        try
        {
            Debug.Log("[VIVOX] Init Vivox");
            Debug.Log("[VIVOX] Init Vivox");
            if (IsHost)
            {
                var name = MultiplayerHost.Lobby.Id;
                GameChannelName.Value = name;
            }
            if (IsClient)
            {
                channelName = GameChannelName.Value.ToString();
                GameChannelName.OnValueChanged += OnChange;
            }
            await Login();
            Debug.Log("[VIVOX] LOGGED IN");
            channelName = GameChannelName.Value.ToString();
            await join3DChannelAsync();
            Debug.Log("[VIVOX] END INIT");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to init Vivox: " + e.Message);
        }
    }

    private void OnChange(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        channelName = newValue.ToString();
    }

    float _nextPosUpdate = 0;
    private void Update()
    {
        if (Time.time > _nextPosUpdate)
        {
            _nextPosUpdate = Time.time + 0.3f;
            if (!loggedIn) return;
            if (!inChannel) return;
            UpdatePlayerHead();
        }
    }
    public async Task join3DChannelAsync()
    {
        try
        {
            // var test = new Channel3DProperties(32, 1, 1, AudioFadeModel.InverseByDistance);
            await VivoxService.Instance.LoginAsync();
            await VivoxService.Instance.EnableAutoVoiceActivityDetectionAsync();
            await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.AudioOnly, new ChannelOptions() {
                MakeActiveChannelUponJoining = true,
            });

            Debug.Log("Joined 3D channel : " + channelName);
            inChannel = true;
            UpdatePlayerHead();
        }
        catch (Exception e)
        {
            inChannel = false;
            Debug.LogError("Failed to join 3D channel: " + e.Message);
        }
    }
    public async Task Login()
    {
        try
        {
            LoginOptions options = new LoginOptions();
            options.PlayerId = NetworkManager.Singleton.LocalClientId.ToString();
            options.DisplayName = "Client " + NetworkManager.Singleton.LocalClientId;
            options.ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond;
            await VivoxService.Instance.LoginAsync(options);
            Debug.Log("Logged in to Vivox");
        }
        catch (Exception e)
        {
            loggedIn = false;
            Debug.LogError("Failed to login to Vivox: " + e.Message);
        }

    }
    private void UpdatePlayerHead()
    {
        Transform head = Player.LocalPlayer?.Camera?.transform ?? null;
        if (head)
        {
            channels = VivoxService.Instance.ActiveChannels.Values.Count.ToString();
            activeInput = VivoxService.Instance.ActiveInputDevice.DeviceName;
            activeOutput = VivoxService.Instance.ActiveOutputDevice.DeviceName;
            // VivoxService.Instance.Set3DPosition(head.gameObject, GameChannelName.Value.ToString(), true);
            
        }
    }

    public override void OnDestroy()
    {
        LogOut();
        base.OnDestroy();
    }

    public async void LogOut()
    {
        await VivoxService.Instance.LogoutAsync();
    }
}