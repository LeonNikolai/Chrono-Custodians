using System.Collections;
using UnityEngine;

enum LeechAdultState
{
    Roaming,
    Patrolling,
    Chasing,
    Searching,
    Dying
}

public class EnemyLeechAdult : Enemy
{
    [SerializeField] private LeechAdultState state;
    [SerializeField] private FieldOfView enemyFOV;
    [SerializeField] private FieldOfView itemFOV;

    [Header("Leech Adult Specific")]
    [SerializeField] private float rushCooldown = 5;
    [SerializeField] private float rushCooldownRandomModifier = 5;
    [SerializeField] private float rushSpeed = 10;
    [SerializeField] private float rushTime = 3;
    [SerializeField] private Transform targetItem;

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

            case LeechAdultState.Patrolling:
                StartCoroutine(Patrolling());
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
        player = enemyFOV.curtarget;
        ResetEnemyToNormal();
        SwitchState(LeechAdultState.Chasing);
    }

    public void PlayerLost()
    {
        if (player == null) return;
        Debug.Log("Player Lost");
        ResetEnemyToNormal();
        SwitchState(LeechAdultState.Searching);
    }

    public void ItemSeen()
    {
        if (player != null) return;
        if (targetItem != null) return;
        targetItem = itemFOV.curtarget.transform;
        if (itemFOV.curtarget.GetComponent<Item>().ItemData.TimePeriods[0].periodName == LevelManager.LoadedScene.TimePeriod.periodName) return;
        ResetEnemyToNormal();
        if (state == LeechAdultState.Roaming)
        {
            SwitchState(LeechAdultState.Patrolling);
        }
    }

    private IEnumerator Patrolling()
    {
        state = LeechAdultState.Patrolling;
        float distance = (targetItem.position - transform.position).magnitude;
        agent.SetDestination(targetItem.position);
        do
        {
            yield return new WaitForSeconds(0.5f);

            distance = (targetItem.position - transform.position).magnitude;
        } while (distance > 0.5f);
        while (state == LeechAdultState.Patrolling)
        {
            float timer = 2;
            float angle = Random.Range(0f, Mathf.PI * 2);
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);
            Vector3 newPos = new Vector3(x, 0, z);
            Quaternion lookPos = Quaternion.LookRotation(newPos);
            while (timer > 0)
            {
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, lookPos, Time.deltaTime * 3);
            }
            if (enemyFOV.canSeeTarget)
            {
                yield break;
            }
        }
        yield return null;
    }

    private IEnumerator Chasing()
    {
        state = LeechAdultState.Chasing;
        player = enemyFOV.curtarget;
        HealthSystem playerHealth = player.GetComponent<HealthSystem>();
        float curAttackCooldown = attackCooldown;
        float curRushTime = rushTime;
        float curRushCooldown = 0;
        float TargetLostTimer = 5;
        float curTargetLostTimer = TargetLostTimer;

        while (state == LeechAdultState.Chasing)
        {
            yield return null;
            if(!App.IsRunning) yield return null;
            curAttackCooldown -= Time.deltaTime;
            StareAtPlayer();
            agent.SetDestination(player.transform.position);

            if (curRushTime > 0)
            {
                agent.speed = rushSpeed;
                curRushTime -= Time.deltaTime;
                if (curRushTime <= 0)
                {
                    curRushCooldown = rushCooldown + Random.Range(-rushCooldownRandomModifier, rushCooldownRandomModifier);
                }
            }

            if (curRushCooldown > 0)
            {
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
                    if (IsServer)
                    {
                        playerHealth.TakeDamageServer(attackDamage);
                    }
                    curAttackCooldown = attackCooldown;
                }
            }

            if (enemyFOV.canSeeTarget == false || enemyFOV.canSeeTarget && enemyFOV.curtarget != player)
            {
                yield return new WaitForEndOfFrame();
                curTargetLostTimer -= Time.deltaTime;
                if (curTargetLostTimer <= 0)
                {
                    ResetEnemyToNormal();
                    SwitchState(LeechAdultState.Searching);
                    yield break;
                }
            }
            else if (!enemyFOV.canSeeTarget)
            {
                curTargetLostTimer = TargetLostTimer;
            }
            if (playerHealth.currentHealth.Value <= 0)
            {
                ResetEnemyToNormal();
                SwitchState(LeechAdultState.Roaming);
                yield break;
            }
        }
    }

    public override IEnumerator Searching()
    {
        //yield return base.Searching();
        yield return null;
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
