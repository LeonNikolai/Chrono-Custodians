using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class Player : NetworkBehaviour, IScanable
{
    // Static variables
    public static Player LocalPlayer = null;
    public static HashSet<Player> AllPlayers = new HashSet<Player>();
    public static Dictionary<ulong, Player> Players = new Dictionary<ulong, Player>();
    public static int PlayerAliveCount => AllPlayers.Count - PlayerDeadCount;
    public static Action OnPlayerDeadCountChanged = delegate { };
    public static Action AllPlayersDead = delegate { };
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
            if (index == spectateIndex) found = true;
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
                Debug.Log("IS NOT Spectating");
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
            if(Health.IsDead) return "Dead worker, less competition :-)";
            var seed = (int)OwnerClientId + NetworkManager.Singleton.ConnectedHostname.GetHashCode();
            return possiblePlayerScanResults.GetString(seed % possiblePlayerScanResults.Length);
        }
    }

    private void Awake()
    {
        if (Camera) Camera.enabled = false;
        if (Input == null) Input = new InputSystem_Actions();
        if (_inventory == null) _inventory = GetComponent<PlayerInventory>();
        if (_movement == null) _movement = GetComponent<PlayerMovement>();
        if (_health == null) _health = GetComponent<HealthSystem>();
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
        }
        Health.onDeath.AddListener(OnHealthChanged);
        AllPlayers.Add(this);
        Players.Add(OwnerClientId, this);
        base.OnNetworkSpawn();
        Camera.enabled = IsOwner;
        UpdateInteractionState();
        OnHealthChanged(false);
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

    public override void OnDestroy()
    {
        if (OwnerClientId == NetworkManager.Singleton.LocalClientId || IsLocalPlayer)
        {
            LocalPlayer = null;
            Input.Player.Disable();
        }
        AllPlayers.Remove(this);
        Players.Remove(OwnerClientId);
    }

    public void OnScan(Player player)
    {

    }
}