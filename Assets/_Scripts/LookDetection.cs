using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class LookDetection : NetworkBehaviour
{
    [SerializeField] private Transform[] posToCheckFrom = new Transform[0];
    [SerializeField] private LayerMask obstructionLayers;
    [SerializeField] private EnemyMannequin enemy;
    bool isRendered = false;

    private void OnBecameVisible()
    {
        if (IsServer && !IsHost) return;
        if (NetworkManager == null || enemy == null) return;
        isRendered = true;
        StartCoroutine(CheckObstruction());
    }

    private void OnBecameInvisible()
    {
        if (IsServer && !IsHost) return;
        if (NetworkManager == null || enemy == null) return;
        Debug.Log("isn't looking");
        isRendered = false;
        StopAllCoroutines();
        if(!IsBeingDestroyed && enemy.IsSpawned) {
            enemy.PlayerStopLookingRPC(NetworkManager.Singleton.LocalClientId);
        }
    }

    bool IsBeingDestroyed = false;
    public override void OnDestroy() {
        IsBeingDestroyed = true;
        StopAllCoroutines();
    }

    private IEnumerator CheckObstruction()
    {
        while (isRendered)
        {
            yield return null;
            if(IsBeingDestroyed) yield break;

            bool isRegistered = false;
            foreach (var pos in posToCheckFrom)
            {
                Vector3 vectorToPlayer = (Camera.main.transform.position - pos.position);
                if (!Physics.Raycast(pos.position, vectorToPlayer.normalized, out RaycastHit hit, vectorToPlayer.magnitude, obstructionLayers))
                {
                    isRegistered = true;  // Visible and unobstructed
                    break;
                }
                else
                {
                }
            }
            if (isRegistered)
            {
                // Debug.Log("Can see enemy");
                enemy.PlayerStartLookingRPC(NetworkManager.Singleton.LocalClientId);
            }
            else
            {
                // Debug.Log("Enemy obstructed");
                enemy.PlayerStopLookingRPC(NetworkManager.Singleton.LocalClientId);
            }

        }
    }
}
