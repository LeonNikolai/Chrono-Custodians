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

    public GameObject player;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstructionMask;

    [SerializeField] private UnityEvent OnPlayerSeen;
    public bool canSeePlayer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        if (!IsServer) yield break;
        float delay = 0.5f;
        WaitForSeconds wait = new WaitForSeconds(delay);

        while (true)
        {
            Debug.Log("FOV check");
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            float closestDistance = Vector3.Distance(transform.position, rangeChecks[0].transform.position);
            player = rangeChecks[0].gameObject;
            for (int i = 1; i < rangeChecks.Length; i++)
            {
                float newDistance = Vector3.Distance(transform.position, rangeChecks[i].transform.position);
                if (closestDistance > newDistance)
                {
                    closestDistance = newDistance;
                    player = rangeChecks[i].gameObject;
                }
            }

            Transform target = player.transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    canSeePlayer = true;
                    OnPlayerSeen.Invoke();
                }
                else
                {
                    canSeePlayer = false;
                    player = null;
                }

            }
            else
            {
                canSeePlayer = false;
                player = null;
            }
        }
        else if (canSeePlayer)
        {
            canSeePlayer = false;
            player = null;
        }
    }
}
