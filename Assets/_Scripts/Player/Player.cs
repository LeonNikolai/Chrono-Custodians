using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Player : NetworkBehaviour
{
    public static Player LocalPlayer = null;
    public static HashSet<Player> AllPlayers = new HashSet<Player>();
    public static InputSystem_Actions Input = null;

    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == NetworkManager.Singleton.LocalClientId || IsLocalPlayer)
        {
            LocalPlayer = this;
            Input = new InputSystem_Actions();
            Input.Player.Enable();
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