using UnityEngine;
using Unity.Netcode;
using Unity.Services.Vivox;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class VivoxPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject localPlayerHead;
    private Vector3 lastPlayerHeadPos;

    private string gameChannelName = "hubbabubba";
    private bool isIn3DChannel = false;
    Channel3DProperties player3DProperties;

    private int clientID;
    [SerializeField] private int newVolumeMinusPlus50 = 0;

    private float _nextPosUpdate;

    private void Start()
    {
        if (IsLocalPlayer)
        {
            InitializeAsync();

            VivoxService.Instance.LoggedIn += onLoggedIn;
            VivoxService.Instance.LoggedOut += onLoggedOut;
            VivoxService.Instance.ChannelJoined += Instance_ChannelJoined;
            VivoxService.Instance.ChannelLeft += Instance_ChannelLeft;
        }
    }

    private void Instance_ChannelLeft(string obj)
    {
        Debug.Log("Vivox: " + clientID + " has left channel " + obj);
    }

    private void Instance_ChannelJoined(string obj)
    {
        Debug.Log("Vivox: " + clientID + " has joined channel " + obj);

    }

    private void Update()
    {
        if (isIn3DChannel && IsLocalPlayer)
        {
            if (Time.time > _nextPosUpdate)
            {
                updatePlayer3DPos();
                _nextPosUpdate += 0.3f;
            }

        }
    }

    private void updatePlayer3DPos()
    {
        VivoxService.Instance.Set3DPosition(localPlayerHead, gameChannelName);
        Debug.Log(VivoxService.Instance.ActiveChannels.Count);
        if (localPlayerHead.transform.position != lastPlayerHeadPos)
        {
            lastPlayerHeadPos = localPlayerHead.transform.position;
        }
    }

    async void InitializeAsync()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        await VivoxService.Instance.InitializeAsync();

        Debug.Log("Vivox: Successfully Initialized");
    }

    public async void LoginToVivoxAsync()
    {
        if (IsLocalPlayer)
        {
            clientID = (int)NetworkManager.Singleton.LocalClientId;
            
            LoginOptions options = new LoginOptions();
            options.DisplayName = "Client" + clientID;
            await VivoxService.Instance.LoginAsync(options);

            join3DChannelAsync();
        }
    }

    public async void join3DChannelAsync()
    {
        await VivoxService.Instance.JoinPositionalChannelAsync(gameChannelName, ChatCapability.AudioOnly, player3DProperties);
        
        isIn3DChannel = true;
        Debug.Log("Vivox: Successfully joined 3D Voice Channel");
    }

    private void onLoggedIn()
    {
        if (VivoxService.Instance.IsLoggedIn)
        {
            Debug.Log("Vivox:" + clientID + " logged in successfully");
        }
        else
        {
            Debug.Log("Vivox: Client could not sign into Vivox");
        }
    }

    private void onLoggedOut()
    {
        VivoxService.Instance.LeaveAllChannelsAsync();
        VivoxService.Instance.LogoutAsync();
        Debug.Log("Vivox: " + clientID + " logged out");
    }

    public void setPlayerHeadPos(GameObject playerHeadPos)
    {
        if (localPlayerHead == null)
        {
            localPlayerHead = playerHeadPos;
        }
    }

    public void UpdateVolume()
    {
        VivoxService.Instance.SetChannelVolumeAsync(gameChannelName, newVolumeMinusPlus50);
    }

}
