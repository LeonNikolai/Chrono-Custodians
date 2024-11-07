using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum MannequinState
{
    Roaming,
    Chasing,
    Searching
}


public class EnemyMannequin : Enemy
{
    [SerializeField] private MannequinState state;
    [SerializeField] private FieldOfView enemyFOV;

    private NetworkVariable<bool> isBeingLookedAt = new NetworkVariable<bool>(false);
    private HashSet<ulong> playersLooking = new HashSet<ulong>();

    [Header("Mannequin Specific Parameters")]
    [SerializeField] private Animator anim;
    [SerializeField] private AnimationClip[] poses;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        StartRoaming();
    }

    private void SwitchState(MannequinState _state)
    {
        if (!IsServer) return;
        if (isRoaming && _state != MannequinState.Roaming)
        {
            StopRoaming();
        }
        if (state == _state) return;
        switch (_state)
        {
            case MannequinState.Roaming:
                StartRoaming();
                state = _state;
                break;

            case MannequinState.Searching:

                break;

            case MannequinState.Chasing:
                StartCoroutine(Chasing());
                break;

        }
    }

    private void Update()
    {
        
    }

    public void PlayerSeen()
    {
        if (state == MannequinState.Roaming)
        {
            SwitchState(MannequinState.Chasing);
        }
    }

    private IEnumerator Chasing()
    {
        state = MannequinState.Chasing;
        float lookTime = 0;
        bool isRotating = false;
        int currentPoseIndex = 0;
        while (state == MannequinState.Chasing)
        {
            yield return null;
            if (enemyFOV.canSeeTarget)
            {
                if (playersLooking.Count == 0)
                {
                    Debug.Log("Stopped");
                    agent.SetDestination(enemyFOV.curtarget.transform.position);
                    if (agent.isStopped)
                    {
                        agent.isStopped = false;
                        lookTime = 0;
                        GetComponentInChildren<MannequinHead>().StartRotating(false);
                    }
                    isRotating = false;
                }
                else
                {
                    Debug.Log("Started");
                    if (!agent.isStopped)
                    {
                        anim.enabled = true;
                        int randomPoseIndex = Random.Range(0, poses.Length);
                        while (randomPoseIndex == currentPoseIndex)
                        {
                            randomPoseIndex = Random.Range(0, poses.Length);
                        }
                        string poseName = poses[Random.Range(0, poses.Length)].name;
                        currentPoseIndex = randomPoseIndex;
                        anim.enabled = false;
                        anim.CrossFade(poseName, 0, 0);
                        agent.SetDestination(transform.position);
                        agent.velocity = Vector3.zero;
                        agent.isStopped = true;
                    }
                    lookTime += Time.deltaTime;
                    if (lookTime > 5 && !isRotating)
                    {
                        isRotating = true;
                        GetComponentInChildren<MannequinHead>().StartRotating(true);
                    }
                }

            }
            else
            {

            }
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerStartLookingRPC(ulong playerId)
    {
        if (!playersLooking.Contains(playerId))
        {
            playersLooking.Add(playerId);
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerStopLookingRPC(ulong playerId)
    {
        if (playersLooking.Contains(playerId))
        {
            playersLooking.Remove(playerId);
        }
    }

    public override void ResetEnemyToNormal()
    {
        base.ResetEnemyToNormal();
        player = null;
    }
}
