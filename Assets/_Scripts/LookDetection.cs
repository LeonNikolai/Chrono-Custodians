using UnityEngine;
using Unity.Netcode;

public class LookDetection : NetworkBehaviour
{
    [SerializeField] private EnemyMannequin enemy;

    private void OnBecameVisible()
    {
        if (IsServer && !IsHost) return;
        if (NetworkManager == null || enemy == null) return;
        enemy.PlayerStartLookingRPC(NetworkManager.Singleton.LocalClientId);
    }

    private void OnBecameInvisible()
    {
        if (IsServer && !IsHost) return;
        if (NetworkManager == null || enemy == null) return;
        enemy.PlayerStopLookingRPC(NetworkManager.Singleton.LocalClientId);
    }

}
