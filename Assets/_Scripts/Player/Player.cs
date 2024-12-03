using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class Player : NetworkBehaviour, IScanable
{
    // Static variables
    public static Player LocalPlayer = null;
    public NetworkVariable<LocationType> _location = new NetworkVariable<LocationType>(LocationType.InsideShip, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public LocationType Location
    {
        get => _location.Value;
        set
        {
            if (IsOwner)
            {
                _location.Value = value;
            }
        }
    }
    public static HashSet<Player> AllPlayers = new HashSet<Player>();
    public static Dictionary<ulong, Player> Players = new Dictionary<ulong, Player>();
    public static int PlayerAliveCount => AllPlayers.Count - PlayerDeadCount;
    public static Action OnPlayerDeadCountChanged = delegate { };
    public static Action AllPlayersDead = delegate { };
    public static Action RenderChanged = delegate { };

    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>("Player", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public string PlayerName => playerName.Value.ToString();


    public static bool AllPlayersAreDead => PlayerDeadCount == AllPlayers.Count;
    static int playerDeadCount = -1;
    public static int PlayerDeadCount
    {
        get
        {
            return playerDeadCount;
        }
        set
        {
            bool changed = playerDeadCount != value;
            playerDeadCount = value;
            if (changed)
            {
                OnPlayerDeadCountChanged.Invoke();
                if (playerDeadCount == AllPlayers.Count)
                {
                    AllPlayersDead.Invoke();
                }
            }
        }
    }
    public static InputSystem_Actions Input = null;

    // Serialized fields
    [Header("Player Components")]
    [SerializeField] PlayerInventory _inventory;
    [SerializeField] PlayerMovement _movement;
    [SerializeField] HealthSystem _health;
    [SerializeField] CinemachineCamera _camera;
    public CinemachineCamera Camera => _camera;
    public PlayerInventory Inventory => _inventory;
    public PlayerMovement Movement => _movement;
    public HealthSystem Health => _health;
    public Transform HeadTransform => Movement.CameraTransform;

    public NetworkVariable<bool> PlayerIsInMenu = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> PlayerIsSpectating = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private static bool inMenu;
    public static bool InMenu
    {
        get => inMenu;
        set
        {
            inMenu = value;
            if (LocalPlayer) LocalPlayer.PlayerIsInMenu.Value = value;
            UpdateInteractionState();
            UpdateCamera();
        }
    }
    public static bool isSpectating;
    public static bool IsSpectating
    {
        get => isSpectating;
        set
        {
            isSpectating = value;
            if (value == true)
            {
                JoinSpectate();
                Hud.SpectateHud.SetActive(true);
                Hud.SpectateUsername = LocalPlayer.PlayerName;
            }
            else
            {
                LeaveSpectate();
                Hud.SpectateHud.SetActive(false);
            }
            if (LocalPlayer) LocalPlayer.PlayerIsSpectating.Value = value;
            UpdateInteractionState();
        }
    }
    public static int spectateIndex;
    public static int SpectateIndex
    {
        get => spectateIndex;
        set
        {
            spectateIndex = value;
            if (spectateIndex < 0) spectateIndex = AllPlayers.Count - 1;
            if (spectateIndex >= AllPlayers.Count) spectateIndex = 0;
            Debug.Log($"Spectating {AllPlayers.Count} players, now spectating {spectateIndex}");
            UpdateCamera();
        }
    }

    private static async void JoinSpectate()
    {
        await VivoxManager.instance.JoinSpectateChannelAsync();
    }
    private static async void LeaveSpectate()
    {
        await VivoxManager.instance.LeaveSpectateChannelAsync();
    }

    private static string currentSpectateTarget = "";
    private string CurrentSpectateTarget 
    { 
        get 
        {  
            return currentSpectateTarget; 
        } 
    }

    private static void UpdateCamera()
    {
        if (!isSpectating)
        {
            foreach (var player in AllPlayers)
            {
                player.Camera.enabled = false;
            }
            if (LocalPlayer) LocalPlayer.Camera.enabled = true;
            return;
        }
        int index = 0;
        bool found = false;
        foreach (var player in AllPlayers)
        {
            player.Camera.enabled = index == spectateIndex;
            if (index == spectateIndex)
            {
                found = true;
                currentSpectateTarget = player.PlayerName;
                Hud.SpectateUsername = currentSpectateTarget;
            }
            index++;
        }
        if (found == false)
        {
            SpectateIndex = 0;
            Debug.LogWarning("No player found to spectate");
        }
    }

    public static void UpdateInteractionState()
    {
        if (inMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            DisableGameInput();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            EnabableGameInput();
        }
    }

    private static void EnabableGameInput()
    {
        if (IsSpectating)
        {
            if (Input != null)
            {
                Input.Player.Disable();
                Input.Spectator.Enable();
            }
            Hud.Hidden = true;
            Debug.Log("IS Spectating");
        }
        else
        {
            Hud.Hidden = false;
            if (Input != null)
            {
                Input.Player.Enable();
                Input.Spectator.Disable();
            }
        }
    }

    private static void DisableGameInput()
    {
        if (Input != null)
        {
            Input.Player.Disable();
            Input.Spectator.Disable();
        }
    }

    [SerializeField] LocalizedStringGroup possiblePlayerScanResults;
    public string ScanTitle => "Player" + (Health.IsDead ? " (Dead)" : "");
    public string ScanResult
    {
        get
        {
            if (Health.IsDead) return "Dead worker, less competition :-)";
            var seed = (int)OwnerClientId + NetworkManager.Singleton.ConnectedHostname.GetHashCode();
            return possiblePlayerScanResults.GetString(seed % possiblePlayerScanResults.Length);
        }
    }


    float originalReflection;
    float originalAmbientIntensity;
    private void Awake()
    {
        if (Camera) Camera.enabled = false;
        if (Input == null) Input = new InputSystem_Actions();
        if (_inventory == null) _inventory = GetComponent<PlayerInventory>();
        if (_movement == null) _movement = GetComponent<PlayerMovement>();
        if (_health == null) _health = GetComponent<HealthSystem>();
        originalReflection = RenderSettings.reflectionIntensity;
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        UpdateInteractionState();
    }

    private void SpectatePrevious(InputAction.CallbackContext context)
    {
        SpectateIndex--;
    }

    private void SpectateNext(InputAction.CallbackContext context)
    {
        SpectateIndex++;
    }

    private void Start()
    {
        UpdateInteractionState();
    }

    public Action OnSpawned = delegate { };
    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == NetworkManager.Singleton.LocalClientId || IsLocalPlayer)
        {
            LocalPlayer = this;
            Input.Player.Enable();
            InMenu = false;
            Input.Spectator.SpectateNextPerson.performed += SpectateNext;
            Input.Spectator.SpectatePreviousPerson.performed += SpectatePrevious;
            Health.onDeath.AddListener(OnSpectatingChanged);
            playerName.Value = CustomUserData.PlayerName;
            GameManager.RenderingUpdate += RenderThisPlayersWorldView;
            RenderThisPlayersWorldView();
        }
        OnSpawned.Invoke();
        Health.onDeath.AddListener(OnHealthChanged);
        AllPlayers.Add(this);
        Players.Add(OwnerClientId, this);
        base.OnNetworkSpawn();
        Camera.enabled = IsOwner;
        _location.OnValueChanged += LocationChanged;

        UpdateInteractionState();
        OnHealthChanged(false);
    }

    private void LocationChanged(LocationType previousValue, LocationType newValue)
    {
        if (IsOwner)
        {
            RenderThisPlayersWorldView();
            RenderChanged.Invoke();
        }
    }

    private void RenderThisPlayersWorldView()
    {
        if (Location == LocationType.Inside)
        {
            if (GameManager.InsideRendering != null)
            {
                GameManager.InsideRendering.Apply();
                return;
            }
            RenderSettings.fog = true;
            if (RenderSettings.fog)
            {
                RenderSettings.fogColor = Color.black;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogEndDistance = 28.17f;
                RenderSettings.fogStartDistance = 0;

            }
            RenderSettings.reflectionIntensity = 0;
            RenderSettings.ambientIntensity = 0.3f;
        }
        if (Location == LocationType.Outside)
        {
            if (GameManager.OutsideRendering != null)
            {
                GameManager.OutsideRendering?.Apply();
                return;
            }
            RenderSettings.fog = false;
            RenderSettings.ambientIntensity = originalAmbientIntensity;
            RenderSettings.reflectionIntensity = originalReflection;
        }
        if (Location == LocationType.InsideShip)
        {
            if (GameManager.ShipRendering != null)
            {
                GameManager.ShipRendering?.Apply();
                return;
            }
            RenderSettings.fog = false; 
            RenderSettings.ambientIntensity = originalAmbientIntensity;
            RenderSettings.reflectionIntensity = originalReflection;
        }
    }

    private void OnHealthChanged(bool arg0)
    {
        int deadPlayerCount = 0;
        foreach (var player in Player.Players)
        {
            if (player.Value.Health.IsDead == true)
            {
                deadPlayerCount++;
            }
        }
        PlayerDeadCount = deadPlayerCount;
    }

    private void OnSpectatingChanged(bool isDead)
    {
        if (isDead)
        {
            Menu.Close();
        }
        IsSpectating = isDead;
        UpdateInteractionState();
    }
    public static void RespawnAll()
    {
        foreach (var player in AllPlayers)
        {
            if(player.Health.IsDead) {
                player.Health.currentHealth.Value = player.Health.maxHealth.Value;
            }
            player.Movement.TeleportToSpawnRpc();
        }
    }
    public override void OnDestroy()
    {
        if (OwnerClientId == NetworkManager.Singleton?.LocalClientId || IsLocalPlayer)
        {
            Input.Player.Disable();
            LocalPlayer = null;
        }
        Health.onDeath.RemoveListener(OnSpectatingChanged);
        Health.onDeath.RemoveListener(OnHealthChanged);
        GameManager.RenderingUpdate -= RenderThisPlayersWorldView;
        AllPlayers.Remove(this);
        Players.Remove(OwnerClientId);
        base.OnDestroy();
    }

    public void OnScan(Player player)
    {

    }

    public static CustomUserData CustomUserData = new CustomUserData();
}