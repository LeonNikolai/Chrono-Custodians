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
        Debug.Log("Is seen");
        isRendered = true;
        StartCoroutine(CheckObstruction());
    }

    private void OnBecameInvisible()
    {
        if (IsServer && !IsHost) return;
        if (NetworkManager == null || enemy == null) return;
        Debug.Log("isn't looking");
        canSee = false;
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
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found!");
            yield break;
        }

        Camera mainCamera = Camera.main; // Cache Camera.main for performance
        while (isRendered && enemy.IsSpawned)
        {
            yield return null; // Wait until the next frame
            if (IsBeingDestroyed) yield break;

            bool canSeeEnemy = false;
            foreach (var pos in posToCheckFrom)
            {
                Vector3 vectorFromPlayer = pos.position - mainCamera.transform.position;
                float distance = vectorFromPlayer.magnitude; // Cache the magnitude

                // Perform raycast to check visibility
                if (!Physics.Raycast(mainCamera.transform.position, vectorFromPlayer.normalized, out RaycastHit hit, distance, obstructionLayers))
                {
                    canSeeEnemy = true;  // Enemy is visible and unobstructed
                    break; // No need to check further
                }
            }

            // Handle visibility state changes
            if (canSeeEnemy)
            {
                if (!canSee)
                {
                    canSee = true;
                    enemy.PlayerStartLookingRPC(NetworkManager.Singleton.LocalClientId);
                }
            }
            else
            {
                if (canSee)
                {
                    canSee = false;
                    enemy.PlayerStopLookingRPC(NetworkManager.Singleton.LocalClientId);
                }
            }
        }
    }

}
