using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Player : NetworkBehaviour
{
    public static Player LocalPlayer = null;
    public static HashSet<Player> AllPlayers = new HashSet<Player>();
    public static InputSystem_Actions Input = null;
    private static bool inMenu;
    public static bool InMenu {
        get => inMenu;
        set {
            inMenu = value;
            if (inMenu)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if(Input != null)Input.Player.Disable();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if(Input != null) Input.Player.Enable();
            }
        }
    }
    private void Awake() {
        if(Input == null) Input = new InputSystem_Actions();
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
    }
}