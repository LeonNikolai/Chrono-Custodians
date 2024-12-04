using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-5)]
public class VivoxManager : MonoBehaviour
{
    public static VivoxManager instance;

    public string ChannelName => "Game" + GameManager.instance.GameID;
    public string SpectateChannel = "Spectate" + GameManager.instance.GameID;
    public string channels = "";
    public string activeInput = "";
    public string activeOutput = "";
    public string MainChannel = "Main" + GameManager.instance.GameID;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else if (instance != null && instance != this)
        {
            Destroy(this);
        }
    }

    async void Start()
    {
        await ServiceInitalizer.InitalizeVivoxServices();
        VivoxService.Instance.LoggedIn += onLoggedIn;
        VivoxService.Instance.LoggedOut += onLoggedOut;
        VivoxService.Instance.ChannelJoined += onChannelJoined;
        VivoxService.Instance.ChannelLeft += onChannelLeft;
        VivoxService.Instance.ParticipantAddedToChannel += AddedParticipant;
        VivoxService.Instance.ParticipantRemovedFromChannel += RemovedParticipant;
        await Login();
        await Join3DChannelAsync();
    }
    public void OnDestroy()
    {
        LogOut();
    }

    public async void LogOut()
    {
        await VivoxService.Instance.LeaveAllChannelsAsync();
        await VivoxService.Instance.LogoutAsync();
    }

    private void RemovedParticipant(VivoxParticipant participant)
    {
        Debug.Log("[VIVOX] Removed participant : " + participant.DisplayName);
    }

    private void AddedParticipant(VivoxParticipant participant)
    {
        Debug.Log("[VIVOX] Added participant : " + participant.DisplayName);
    }

    private void onChannelJoined(string obj)
    {
        Debug.Log("[VIVOX] Joined channel : " + obj);
        inChannel = true;
    }

    private void onChannelLeft(string obj)
    {
        Debug.Log("[VIVOX] Left channel : " + obj);
        inChannel = false;
    }

    private void onLoggedOut()
    {
        loggedIn = false;
    }

    private void onLoggedIn()
    {
        loggedIn = true;
    }

    public bool loggedIn = false;
    public bool inChannel = false;
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
    public async Task Join3DChannelAsync()
    {
        try
        {
            var test = new Channel3DProperties(50, 15, 10f, AudioFadeModel.InverseByDistance);
            await VivoxService.Instance.JoinPositionalChannelAsync(ChannelName, ChatCapability.AudioOnly, test);
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.Single, ChannelName);
            inChannel = true;
            UpdatePlayerHead();
        }
        catch (Exception e)
        {
            inChannel = false;
            Debug.LogError("Failed to join 3D channel: " + e.Message);
        }
    }

    public async Task JoinSpectateChannelAsync()
    {
        try
        {
            await VivoxService.Instance.JoinGroupChannelAsync(SpectateChannel, ChatCapability.AudioOnly);
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.Single, SpectateChannel);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to join Spectate Channel");
        }
    }

    public async Task LeaveSpectateChannelAsync()
    {
        try
        {
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.Single, ChannelName);
            await VivoxService.Instance.LeaveChannelAsync(SpectateChannel);
        }
        catch(Exception e)
        {
            Debug.Log("Could not leave Spectate Channel");
        }
    }

    public static string DisplayName(ulong clientId)
    {
        return "VIVOX_" + clientId;
    }
    public async Task Login()
    {
        if (VivoxService.Instance.IsLoggedIn)
        {
            return;
        }
        try
        {
            LoginOptions options = new LoginOptions();
            options.DisplayName = DisplayName(NetworkManager.Singleton.LocalClientId);
            options.ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond;
            await VivoxService.Instance.LoginAsync(options);
        }
        catch (Exception e)
        {
            loggedIn = false;
            Debug.LogError("Failed to login to Vivox: " + e.Message);
        }
    }

    Vector3 lastPos = Vector3.zero;
    private void UpdatePlayerHead()
    {
        Transform head = Camera.main.transform ?? null;
        if (head)
        {
            channels = VivoxService.Instance.ActiveChannels.Values.Count.ToString();
            activeInput = VivoxService.Instance.ActiveInputDevice.DeviceName;
            activeOutput = VivoxService.Instance.ActiveOutputDevice.DeviceName;
            if (Math.Sqrt(Vector3.Distance(lastPos, head.position)) > 0.1f)
            {
                lastPos = head.position;
                VivoxService.Instance.Set3DPosition(head.gameObject, ChannelName, true);
            }
        }
    }


}