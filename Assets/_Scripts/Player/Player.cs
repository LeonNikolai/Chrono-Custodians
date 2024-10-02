using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Player : NetworkBehaviour, IScanable
{
    // Static variables
    public static Player LocalPlayer = null;
    public static HashSet<Player> AllPlayers = new HashSet<Player>();
    public static Dictionary<ulong, Player> Players = new Dictionary<ulong, Player>();
    public static InputSystem_Actions Input = null;

    // Serialized fields
    [Header("Player Components")]
    [SerializeField] PlayerInventory _inventory;
    [SerializeField] PlayerMovement _movement;
    [SerializeField] HealthSystem _health;

    public PlayerInventory Inventory => _inventory;
    public PlayerMovement Movement => _movement;
    public HealthSystem Health => _health;
    public Transform HeadTransform => Movement.CameraTransform;

    // Network variables
    public NetworkVariable<float> Age = new NetworkVariable<float>(20);

    private static bool inMenu;
    public static bool InMenu
    {
        get => inMenu;
        set
        {
            inMenu = value;
            if (inMenu)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (Input != null) Input.Player.Disable();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                if (Input != null) Input.Player.Enable();
            }
        }
    }
    [SerializeField] LocalizedStringGroup possiblePlayerScanResults;
    public string ScanTitle => "Player";
    public string ScanResult {
        get {
            var seed = (int)OwnerClientId + NetworkManager.Singleton.ConnectedHostname.GetHashCode();
            return possiblePlayerScanResults.GetString(seed % possiblePlayerScanResults.Length);
        }
    }

    private void Awake()
    {
        if (Input == null) Input = new InputSystem_Actions();
        if(_inventory == null) _inventory = GetComponent<PlayerInventory>();
        if(_movement == null) _movement = GetComponent<PlayerMovement>();
        if(_health == null) _health = GetComponent<HealthSystem>();
    }

    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == NetworkManager.Singleton.LocalClientId || IsLocalPlayer)
        {
            LocalPlayer = this;
            Input.Player.Enable();
            InMenu = false;
        }
        AllPlayers.Add(this);
        Players.Add(OwnerClientId, this);
        base.OnNetworkSpawn();
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