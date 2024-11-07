using UnityEngine;
using Unity.Netcode;
using System.Collections.Specialized;

public class LookDetection : NetworkBehaviour
{
    [SerializeField] private EnemyMannequin enemy;

    private void OnBecameVisible()
    {
        if (IsServer && !IsHost) return;
        if (enemy == null) return;
        enemy.PlayerStartLookingRPC(NetworkManager.Singleton.LocalClientId);
    }

    private void OnBecameInvisible()
    {
        if (IsServer && !IsHost) return;
        if (enemy == null) return;

        enemy.PlayerStopLookingRPC(NetworkManager.Singleton.LocalClientId);
    }

}
