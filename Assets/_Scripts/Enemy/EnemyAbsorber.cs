using System.Collections;
using System.IO;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering.UI;

enum AbsorberState
{
    Roaming,
    Stalking, // The Absorber has discovered a player. In this behavior it will stop and observe the player and follow them.
    Chasing

}

public class EnemyAbsorber : Enemy
{
    [Header("Absorber Specific")]
    [SerializeField] private AbsorberState state;
    [SerializeField] private GameObject player;
    private HealthSystem playerHealth;
    [SerializeField] private FieldOfView enemyFOV;


    [SerializeField] private float chaseCountdown;
    private float curChaseCountdown;
    private float curAttackCooldown;
    private float normalSpeed;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        normalSpeed = agent.speed;

    }

    private void SwitchState(AbsorberState _state)
    {
        if (!IsServer) return;
        switch(_state)
        {
            case AbsorberState.Roaming:
                state = AbsorberState.Roaming;
                StartRoaming();
                break;

            case AbsorberState.Stalking:
                StopRoaming();
                StartCoroutine(Stalking());
                break;

            case AbsorberState.Chasing:
                StopRoaming();
                StartCoroutine(Chasing());
                break;
        }
    }
    
    public override void Attack()
    {
    }

    public void PlayerSpotted()
    {
        if (state == AbsorberState.Roaming && player == null)
        {
            player = enemyFOV.player;
            SwitchState(AbsorberState.Stalking);
        }
    }

    IEnumerator Stalking()
    {
        Debug.Log("Stalking");
        state = AbsorberState.Stalking;
        curChaseCountdown = chaseCountdown;
        while (state == AbsorberState.Stalking)
        {
            StareAtPlayer();
            yield return null;
            if (enemyFOV.canSeePlayer)
            {
                chaseCountdown -= Time.deltaTime;
                Debug.Log("Staring at player");
                agent.speed = 0;
            }
            else
            {
                Debug.Log("Moving to player");
                agent.speed = normalSpeed;
                if (agent.destination != player.transform.position)
                {
                    agent.SetDestination(player.transform.position);
                }
            }

            if (chaseCountdown <= 0) SwitchState(AbsorberState.Chasing);
        }
        yield break;
    }

    IEnumerator Chasing() // When chasing, the Absorber will relentlessly go towards the player at increased speed.
    {
        // Place any transitionary stuff here. The state change has to be at the end
        if (playerHealth == null) playerHealth = player.GetComponent<HealthSystem>();
        state = AbsorberState.Chasing;
        playerHealth.onDeath.AddListener(TargetPlayerDied);
        Debug.Log("IM COMING FOR YA BICH!");
        agent.speed = normalSpeed * 2;
        agent.stoppingDistance = 2;
        // Place any chasing logic here
        while (state == AbsorberState.Chasing)
        {
            if (curAttackCooldown >= 0)
                curAttackCooldown -= Time.deltaTime;
            yield return null;
            StareAtPlayer();
            agent.SetDestination(player.transform.position);

            if (Vector3.SqrMagnitude(player.transform.position - transform.position) < attackRange * attackRange)
            {
                if (curAttackCooldown <= 0)
                {
                    playerHealth.TakeDamageServerRpc(attackDamage);
                    curAttackCooldown = attackCooldown;
                }
            }
            if (player == null)
            {
                Debug.Log("Player is dead");
                agent.isStopped = true;
                agent.speed = normalSpeed;
                yield return new WaitForSeconds(4);
                SwitchState(AbsorberState.Roaming);
                agent.isStopped = false;
            }
        }
        yield break;
    }

    private void TargetPlayerDied()
    {
        playerHealth.onDeath.RemoveListener(TargetPlayerDied);
        playerHealth = null;
        player = null;
    }

    private void StareAtPlayer()
    {
        if (!player) return;
        Vector3 direction = player.transform.position - transform.position;

        direction.x = 0;
        direction.z = 0;

        // Create the rotation we need to look at the target
        Quaternion lookRotation = Quaternion.LookRotation(player.transform.position - transform.position);

        // Smoothly rotate towards the target
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 40);
    }
}
