using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class FieldOfView : NetworkBehaviour
{
    public float radius;
    [Range(0, 360)]
    public float angle;

    public Transform head;

    public GameObject player;
    private CapsuleCollider playerCollider;
    private int raycastPoints = 3;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstructionMask;

    public UnityEvent OnPlayerSeen;
    public UnityEvent OnPlayerLost;
    public bool canSeePlayer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (head == null)
        {
            head = transform;
        }

        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        if (!IsServer) yield break;
        float delay = 0.5f;
        WaitForSeconds wait = new WaitForSeconds(delay);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(head.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            float closestDistance = Vector3.Distance(head.position, rangeChecks[0].transform.position);
            player = rangeChecks[0].gameObject;
            for (int i = 1; i < rangeChecks.Length; i++)
            {
                float newDistance = Vector3.Distance(head.position, rangeChecks[i].transform.position);
                if (closestDistance > newDistance)
                {
                    closestDistance = newDistance;
                    player = rangeChecks[i].gameObject;
                }
            }

            Transform target = player.transform;
            playerCollider = target.GetComponent<CapsuleCollider>();
            Vector3 directionToTarget = (target.position - head.position).normalized;

            if (Vector3.Angle(head.forward, directionToTarget) < angle * 0.5)
            {
                if (IsPlayerVisible(target))
                {
                    if (!canSeePlayer)
                    {
                        OnPlayerSeen.Invoke();
                    }
                    canSeePlayer = true;
                }
                else
                {
                    if (canSeePlayer)
                    {
                        OnPlayerLost.Invoke();
                    }
                    canSeePlayer = false;
                    player = null;
                    playerCollider = null;
                }
            }
            else
            {
                if (canSeePlayer)
                {
                    if (closestDistance <= 5)
                    {
                        return;
                    }
                    else
                    {
                        OnPlayerLost.Invoke();
                    }
                }
                canSeePlayer = false;
                player = null;
                playerCollider = null;
            }
        }
        else if (canSeePlayer)
        {

            OnPlayerLost.Invoke();
            canSeePlayer = false;
            player = null;
            playerCollider = null;
        }
    }

    private bool IsPlayerVisible(Transform target)
    {
        // Perform multiple raycasts along the player's body to detect visibility
        for (int i = 0; i < raycastPoints; i++)
        {
            // Calculate the point along the player's body for this raycast
            float heightOffset = (playerCollider.height / (raycastPoints - 1)) * i;
            Vector3 raycastOrigin = target.position + Vector3.up * heightOffset;

            Vector3 directionToTarget = (raycastOrigin - head.position).normalized;
            float distanceToTarget = Vector3.Distance(head.position, raycastOrigin);

            // Perform the raycast for this point
            if (!Physics.Raycast(head.position, directionToTarget, distanceToTarget, obstructionMask))
            {
                // If any point is visible, we consider the player visible
                return true;
            }
        }

        // If none of the points are visible, the player is not visible
        return false;
    }


    private void Reset()
    {
        head = this.transform;
    }
}
