using System.Collections;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

enum LeechAdultState
{
    Roaming,
    Chasing,
    Searching,
    Dying
}

public class EnemyLeechAdult : Enemy
{
    [SerializeField] private LeechAdultState state;
    private HealthSystem playerHealth;
    [SerializeField] private FieldOfView enemyFOV;

    [Header("Leech Adult Specific")]
    [SerializeField] private float rushCooldown = 5;
    [SerializeField] private float rushCooldownRandomModifier = 5;
    [SerializeField] private float rushSpeed = 10;
    [SerializeField] private float rushTime = 3;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
    }

    private void SwitchState(LeechAdultState _state)
    {
        if (!IsServer) return;
        if (isRoaming && _state != LeechAdultState.Roaming)
        {
            StopRoaming();
        }
        switch (_state)
        {
            case LeechAdultState.Roaming:
                state = LeechAdultState.Roaming;
                StartCoroutine(Roaming());
                break;

            case LeechAdultState.Chasing:
                StartCoroutine(Chasing());
                break;

            case LeechAdultState.Searching:
                StartCoroutine(Searching());
                break;

            case LeechAdultState.Dying:
                StartCoroutine(Dying());
                break;

        }
    }

    public void PlayerSpotted()
    {
        if (player != null) return;
        player = enemyFOV.player;
        StopAllCoroutines();
        SwitchState(LeechAdultState.Chasing);
    }

    public void PlayerLost()
    {
        if (player == null) return;
        StopAllCoroutines();
        SwitchState(LeechAdultState.Searching);
    }

    private IEnumerator Chasing()
    {
        state = LeechAdultState.Chasing;
        float curAttackCooldown = attackCooldown;

        while (state == LeechAdultState.Chasing)
        {
            agent.speed = rushSpeed;
            yield return new WaitForSeconds(rushTime);

            agent.speed = moveSpeed;
            yield return new WaitForSeconds(rushCooldown + Random.Range(-rushCooldownRandomModifier, rushCooldownRandomModifier));
        }

        agent.SetDestination(player.transform.position);

        if (Vector3.SqrMagnitude(player.transform.position - transform.position) < attackRange * attackRange)
        {
            if (curAttackCooldown <= 0)
            {
                if (IsServer)
                {
                    playerHealth.TakeDamageServer(attackDamage);
                }
                curAttackCooldown = attackCooldown;
            }
        }

        yield return null;
    }

    public override IEnumerator Searching()
    {
        /*
        yield return base.Searching();
        */
        Debug.Log("Searching..");
        yield return new WaitForSeconds(2);
        SwitchState(LeechAdultState.Roaming);

    }

    private IEnumerator Dying()
    {
        yield return null;
    }
}
