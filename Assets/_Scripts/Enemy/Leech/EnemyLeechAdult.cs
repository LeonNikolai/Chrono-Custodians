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
        if (state == _state) return;
        switch (_state)
        {
            case LeechAdultState.Roaming:
                state = LeechAdultState.Roaming;
                StartCoroutine(Roaming());
                break;

            case LeechAdultState.Chasing:
                Debug.Log("Entering Searching State");
                StartCoroutine(Chasing());
                break;

            case LeechAdultState.Searching:
                Debug.Log("Entering Searching State");
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
        Debug.Log("Player Spotted");
        player = enemyFOV.player;
        ResetEnemyToNormal();
        Debug.Log("All Coroutines Stopped");
        SwitchState(LeechAdultState.Chasing);
    }

    public void PlayerLost()
    {
        if (player == null) return;
        Debug.Log("Player Lost");
        ResetEnemyToNormal();
        Debug.Log("All Coroutines Stopped");
        SwitchState(LeechAdultState.Searching);
    }

    private IEnumerator Chasing()
    {
        state = LeechAdultState.Chasing;
        HealthSystem playerHealth = player.GetComponent<HealthSystem>();
        float curAttackCooldown = attackCooldown;
        float curRushTime = rushTime;
        float curRushCooldown = 0;

        Debug.Log("Chasing Before Loop");
        while (state == LeechAdultState.Chasing)
        {
            yield return null;
            curAttackCooldown -= Time.deltaTime;
            Debug.Log("In Chasing Loop");
            StareAtPlayer();
            agent.SetDestination(player.transform.position);

            if (curRushTime > 0)
            {
                Debug.Log("Rushing");
                agent.speed = rushSpeed;
                curRushTime -= Time.deltaTime;
                if (curRushTime <= 0)
                {
                    curRushCooldown = rushCooldown + Random.Range(-rushCooldownRandomModifier, rushCooldownRandomModifier);
                }
            }

            if (curRushCooldown > 0)
            {
                Debug.Log("Rush Cooldown");
                agent.speed = moveSpeed;
                curRushCooldown -= Time.deltaTime;
                if (curRushCooldown <= 0)
                {

                    curRushTime = rushTime;
                }
            }

            if (Vector3.SqrMagnitude(player.transform.position - transform.position) < attackRange * attackRange)
            {
                if (curAttackCooldown <= 0)
                {
                    Debug.Log("Attacking Player");
                    if (IsServer)
                    {
                        playerHealth.TakeDamageServer(attackDamage);
                    }
                    curAttackCooldown = attackCooldown;
                }
            }

            if (enemyFOV.canSeePlayer == false)
            {
                SwitchState(LeechAdultState.Searching);
                yield break;
            }
        }
    }

    public override IEnumerator Searching()
    {
        //yield return base.Searching();
        Debug.Log("Searching..");
        yield return new WaitForSeconds(2); 
        SwitchState(LeechAdultState.Roaming);

    }

    private IEnumerator Dying()
    {
        yield return null;
    }

    public override void ResetEnemyToNormal()
    {
        base.ResetEnemyToNormal();
        player = null;
    }
}
