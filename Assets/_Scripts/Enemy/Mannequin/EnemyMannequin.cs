using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum MannequinState
{
    Roaming,
    Chasing,
    Killing,
    Searching
}


public class EnemyMannequin : Enemy
{
    [SerializeField] private MannequinState state;
    [SerializeField] private FieldOfView enemyFOV;

    private NetworkVariable<bool> isBeingLookedAt = new NetworkVariable<bool>(false);
    private HashSet<ulong> playersLooking = new HashSet<ulong>();

    [Header("Mannequin Specific Parameters")]
    [SerializeField] private float timeToKill = 1f;
    [SerializeField] private Animator anim;
    [SerializeField] private AnimationClip[] poses;
    [SerializeField] private AnimationClip killPose;
    [SerializeField] private AudioSource crunch;
    [SerializeField] private AudioClip crunchClip;

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

            case MannequinState.Killing:
                StartCoroutine(Killing());
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
        float killTime = 0;
        bool isRotating = false;
        bool isSeen = false;
        bool isKilling = false;
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
                        isSeen = false;
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
                        anim.CrossFade(poseName, 0, 0);
                        agent.SetDestination(transform.position);
                        agent.velocity = Vector3.zero;
                        agent.isStopped = true;
                        isSeen = true;
                    }
                    lookTime += Time.deltaTime;
                    if (lookTime > 5 && !isRotating)
                    {
                        anim.enabled = false;
                        isRotating = true;
                        GetComponentInChildren<MannequinHead>().StartRotating(true);
                    }
                }
                if ((enemyFOV.curtarget.transform.position - transform.position).magnitude < 2f && !isSeen)
                {
                    if (!isKilling)
                    {
                        isKilling = true;
                        killTime = timeToKill;
                    }
                    killTime -= Time.deltaTime;
                    if (killTime < 0)
                    {
                        SwitchState(MannequinState.Killing);
                    }
                }
            }
        }
    }

    private IEnumerator Killing()
    {
        var target = enemyFOV.curtarget;
        var targetHealth = target.GetComponent<HealthSystem>();
        if (target == null)
        {
            Debug.Log("No target.. this shouldn't have happened");
        }
        state = MannequinState.Killing;
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        collider.enabled = false;
        transform.position = target.transform.position - (target.transform.forward * 0.64f);
        target.GetComponent<PlayerMovement>().DisableInputsRPC(true);
        anim.CrossFade(killPose.name, 0, 0);

        yield return null;
        // Start playing head crunching sound and kill animation local to the player
        crunch.PlayOneShot(crunchClip);
        targetHealth.TakeDamageServer((int)Mathf.Ceil(targetHealth.currentHealth.Value * 0.5f));
        yield return new WaitForSeconds(1);
        targetHealth.TakeDamageServer((int)Mathf.Ceil(targetHealth.currentHealth.Value * 0.5f));
        yield return new WaitForSeconds(1.25f);
        targetHealth.TakeDamageServer((int)Mathf.Ceil(targetHealth.currentHealth.Value * 0.5f));
        yield return new WaitForSeconds(1.5f);
        targetHealth.TakeDamageServer((int)Mathf.Ceil(targetHealth.currentHealth.Value * 0.5f));
        yield return new WaitForSeconds(0.1f);
        targetHealth.TakeDamageServer(1000);
        // Kill player

        collider.enabled = true;
        SwitchState(MannequinState.Roaming);
        yield return null;

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
