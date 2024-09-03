using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.Events;

public class PlayerConnection : NetworkBehaviour
{
    public static HashSet<PlayerConnection> Connections = new();
    public static UnityEvent<PlayerConnection> OnPlayerConnected = new();
    public static UnityEvent<PlayerConnection> OnPlayerDisconnected = new();
    public bool IsDead { get; private set; }
    public override void OnNetworkSpawn()
    {
        Connections.Add(this);
        OnPlayerConnected.Invoke(this);
        base.OnNetworkSpawn();
    }

    private void Update() {

    }

    public override void OnDestroy()
    {
        Connections.Remove(this);
        OnPlayerDisconnected.Invoke(this);
        base.OnDestroy();
    }
}