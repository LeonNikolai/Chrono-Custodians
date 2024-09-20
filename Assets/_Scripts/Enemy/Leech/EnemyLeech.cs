using System.Collections;
using UnityEngine;

enum LeechState
{
    Roaming,
    Attaching,
    Draining,
    Fleeing,
    Gestating
}

public class EnemyLeech : Enemy
{
    [SerializeField] private LeechState state;
    [SerializeField] private GameObject player;
    private HealthSystem playerHealth;
    [SerializeField] private FieldOfView enemyFOV;
    private float normalSpeed;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        normalSpeed = moveSpeed;
    }

    private void SwitchState(LeechState _state)
    {
        if (isRoaming && _state != LeechState.Roaming)
        {
            StopRoaming();
        }
        switch (_state)
        {
            case LeechState.Roaming:
                state = LeechState.Roaming;
                StartRoaming();
                break;

            case LeechState.Attaching:

                break;

            case LeechState.Draining:

                break;

            case LeechState.Fleeing:

                break;

            case LeechState.Gestating:

                break;

        }
    }

    private IEnumerator Attaching()
    {
        yield return null;
    }

    private IEnumerator Draining()
    {
        yield return null;
    }

    private IEnumerator Fleeing()
    {
        yield return null;
    }

    private IEnumerator Gestating()
    {
        yield return null;
    }
}
