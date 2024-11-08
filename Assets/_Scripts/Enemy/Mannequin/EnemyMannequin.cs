using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum MannequinState
{
    Roaming,
    Idle,
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
    [SerializeField] private GameObject killFace;
    [SerializeField] private GameObject killAnim;
    [SerializeField] private MannequinHead head;
    [SerializeField] private Animator anim;
    [SerializeField] private AnimationClip[] poses;
    [SerializeField] private AnimationClip killPose;
    [SerializeField] private AudioSource crunch;
    [SerializeField] private AudioClip crunchClip;
    [SerializeField] private float chaseTime = 2;

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

            case MannequinState.Idle:
                state = MannequinState.Idle;
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
        if (!IsServer || enemyFOV.curtarget == null) return;
        player = enemyFOV.curtarget;
        if (state == MannequinState.Roaming || state == MannequinState.Idle)
        {
            SwitchState(MannequinState.Chasing);
        }
    }

    private IEnumerator Chasing()
    {
        state = MannequinState.Chasing;
        float lookTime = 0;
        float killTime = 0;
        float curChaseTime = 0;
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
                        isSeen = false;
                        if (killFace.activeInHierarchy)
                        {
                            killFace.SetActive(false);
                        }
                        else
                        {
                            head.StartRotating(false);
                        }
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
                        anim.CrossFadeInFixedTime(poseName, 0, 0);
                        agent.SetDestination(transform.position);
                        Vector3 lookDir = player.transform.position - transform.position;
                        transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
                        agent.velocity = Vector3.zero;
                        agent.isStopped = true;
                        isSeen = true;
                        if (isKilling)
                        {
                            isKilling = false;
                            killFace.SetActive(true);
                            yield return new WaitForSeconds(Time.deltaTime);
                            anim.enabled = false;
                            head.SetHeadToTarget();
                        }
                    }
                    lookTime += Time.deltaTime;
                    if (lookTime > 5 && !isRotating)
                    {
                        anim.enabled = false;
                        isRotating = true;
                        head.StartRotating(true);
                    }
                }
                if ((enemyFOV.curtarget.transform.position - transform.position).magnitude < 2f && !isSeen)
                {
                    if (!isKilling)
                    {
                        isKilling = true;
                        isRotating = true;
                        killFace.SetActive(true);
                        killTime = timeToKill;
                    }
                    killTime -= Time.deltaTime;
                    if (killTime < 0)
                    {
                        SwitchState(MannequinState.Killing);
                    }

                }
            }
            else
            {
                if (curChaseTime < chaseTime)
                {
                    agent.SetDestination(player.transform.position);
                }
                else
                {
                    SwitchState(MannequinState.Roaming);
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
        Vector3 lookDir = target.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        target.GetComponent<PlayerMovement>().DisableInputsRPC(true);
        anim.enabled = true;
        anim.CrossFadeInFixedTime(killPose.name, 0, 0);

        yield return null;
        // Start playing head crunching sound and kill animation local to the player
        SpawnDeathAnimRPC();
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
        if (enemyFOV.canSeeTarget)
        {
            SwitchState(MannequinState.Idle);
        }
        yield return null;

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnDeathAnimRPC()
    {
        Debug.Log("Called SpawnDeathAnimRPC"); 
        if (NetworkManager.LocalClientId == player.GetComponent<Player>().OwnerClientId)
        {
            Debug.Log("Spawned Death Anim");
            Destroy(Instantiate(killAnim, Hud.AnimOverlay.transform), 4);
        }
        if (IsLocalPlayer)
        {
            Debug.Log("IsLocalPlayer is true");

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
