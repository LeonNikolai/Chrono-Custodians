using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class LookDetection : NetworkBehaviour
{
    [SerializeField] private Transform[] posToCheckFrom = new Transform[0];
    [SerializeField] private LayerMask obstructionLayers;
    [SerializeField] private EnemyMannequin enemy;
    bool isRendered = false;
    bool canSee = false;

    private void OnBecameVisible()
    {
        if (IsServer && !IsHost) return;
        if (NetworkManager == null || enemy == null) return;
        if (Player.LocalPlayer.PlayerIsSpectating.Value)
        {
            if (isRendered == false)
            {
                return;
            }
            isRendered = false;
            enemy.PlayerStopLookingRPC(NetworkManager.Singleton.LocalClientId);
            return;
        }
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
        while (isRendered && enemy.IsSpawned)
        {
            yield return null;
            if(IsBeingDestroyed) yield break;

            bool isRegistered = false;
            foreach (var pos in posToCheckFrom)
            {
                Vector3 vectorFromPlayer = (pos.position - Camera.main.transform.position);
                if (!Physics.Raycast(Camera.main.transform.position, vectorFromPlayer.normalized, out RaycastHit hit, vectorFromPlayer.magnitude, obstructionLayers))
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
                if (canSee == true) continue;
                canSee = true;
                enemy.PlayerStartLookingRPC(NetworkManager.Singleton.LocalClientId);
            }
            else
            {

                if (canSee == false) continue;
                // Debug.Log("Enemy obstructed");
                canSee = false;
                enemy.PlayerStopLookingRPC(NetworkManager.Singleton.LocalClientId);
            }

        }
    }
}
