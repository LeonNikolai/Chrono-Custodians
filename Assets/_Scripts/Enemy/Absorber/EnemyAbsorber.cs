using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Video;

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
    private HealthSystem playerHealth;
    [SerializeField] private FieldOfView enemyFOV;


    [SerializeField] private float chaseCountdown;
    private float curChaseCountdown;
    private float curAttackCooldown;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;

    }

    private void SwitchState(AbsorberState _state)
    {
        if (!IsServer) return;
        switch(_state)
        {
            case AbsorberState.Roaming:
                state = AbsorberState.Roaming;
                StartCoroutine(Roaming());
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

    public void PlayerSpotted()
    {
        if (state == AbsorberState.Roaming && player == null)
        {
            player = enemyFOV.curtarget;
            SwitchState(AbsorberState.Stalking);
        }
    }

    public override IEnumerator Roaming()
    {
        return base.Roaming();
    }

    IEnumerator Stalking()
    {
        Debug.Log("Stalking");
        state = AbsorberState.Stalking;
        curChaseCountdown = chaseCountdown;
        while (state == AbsorberState.Stalking)
        {
            StareAtPlayer();
            agent.SetDestination(player.transform.position);
            yield return null;
            if (enemyFOV.canSeeTarget)
            {
                chaseCountdown -= Time.deltaTime;
                Debug.Log("Staring at player");
                agent.speed = 0;
            }
            else
            {
                Debug.Log("Moving to player");
                agent.speed = moveSpeed;
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
        agent.speed = moveSpeed * 2;
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
                    if(IsServer) {
                        playerHealth.TakeDamageServer(attackDamage);
                    }
                    curAttackCooldown = attackCooldown;
                }
            }
            if (player == null)
            {
                Debug.Log("Player is dead");
                agent.isStopped = true;
                agent.speed = moveSpeed;
                yield return new WaitForSeconds(4);
                SwitchState(AbsorberState.Roaming);
                agent.isStopped = false;
            }
        }
        yield break;
    }

    private void TargetPlayerDied(bool dead)
    {
        playerHealth.onDeath.RemoveListener(TargetPlayerDied);
        playerHealth = null;
        player = null;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if(agent != null) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, agent.destination);
            Gizmos.DrawWireSphere(agent.destination, 1);
        }
    }
}
