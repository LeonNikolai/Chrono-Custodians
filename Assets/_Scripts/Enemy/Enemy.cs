using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.AI;

public abstract class Enemy : NetworkBehaviour
{

    [Header("Enemy Base")]
    // Base Statistics
    public bool DrawAttackGizmos = false;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected float attackRange;
    [SerializeField] protected int attackDamage;
    [SerializeField] protected float attackCooldown;
    [SerializeField] protected float moveSpeed;

    protected NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    [Header("Enemy Base AI Behavior")]
    public bool DrawWaypointGizmos = false;

    // AI Pathfinding Behavior Variables
    protected NavMeshAgent agent;
    [SerializeField] protected bool isRoaming = true;
    [SerializeField] private Transform targetWaypoint;
    [SerializeField] private float CheckoffRadius = 5f;
    [SerializeField] private float waypointIgnoreRadius = 15f;
    public List<Transform> waypoints = new List<Transform>();
    public Queue<Transform> visitedWaypoints = new Queue<Transform>();

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [SerializeField] private Material normal, target, visited;
    

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;
            if (!isRoaming) return;
            GetWaypoints();
            SelectNewWaypoint();
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (!isRoaming) return;
        // Checks if the enemy is roaming and the distance between it and the target waypoint is within the checkoff radius
        float sqrDistance = Vector3.SqrMagnitude(transform.position - targetWaypoint.position);
        if (sqrDistance < CheckoffRadius * CheckoffRadius) WaypointReached();
    }

    private void GetWaypoints()
    {
        waypoints = WaypointManager.Instance.GetWaypoints();
        // Enemy should find a waypoint manager and get a list of waypoints that are active.
    }


    protected virtual void WaypointReached()
    {
        // Adds the reached waypoint to a list and selects a new waypoint to go to.
        if (targetWaypoint == null)
        {
            SelectNewWaypoint();
            return;
        }
        waypoints.Remove(targetWaypoint);
        visitedWaypoints.Enqueue(targetWaypoint);
        if (debug)  targetWaypoint.GetComponentInChildren<MeshRenderer>().material = visited;
        targetWaypoint = null;
        SelectNewWaypoint();

    }

    private void SelectNewWaypoint()
    {
        Transform newTarget = null;
        if (waypoints.Count <= 0) return;
        do
        {
            newTarget = waypoints[Random.Range(0, waypoints.Count)];
        } while (Vector3.SqrMagnitude(transform.position - newTarget.position) < waypointIgnoreRadius * waypointIgnoreRadius);

        if (visitedWaypoints.Count > 10)
        {
            Transform tempWaypoint = visitedWaypoints.Dequeue();
            if (debug) tempWaypoint.GetComponentInChildren<MeshRenderer>().material = normal;
            waypoints.Add(tempWaypoint);
        }
        targetWaypoint = newTarget;
        if (debug) targetWaypoint.GetComponentInChildren<MeshRenderer>().material = target;
        MoveToWaypoint();
    }

    private void MoveToWaypoint()
    {
        agent.SetDestination(targetWaypoint.position);
    }


    public abstract void Attack();

    public virtual void StopRoaming()
    {
        isRoaming = false;
        agent.SetDestination(transform.position);
    }

    public virtual void StartRoaming()
    {
        isRoaming = true;
        SelectNewWaypoint();
    }

    private void OnDrawGizmos()
    {
        if (debug)
        {
            if (agent == null || agent.path == null)
                return;

            // Set the color for the path
            Gizmos.color = Color.yellow;

            // Get the corners of the path
            var path = agent.path;
            var corners = path.corners;

            // Draw the path by connecting each corner point with a line
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }

        if (DrawAttackGizmos)
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        if (DrawWaypointGizmos)
        {
            // Draw CheckoffRadius for waypoints
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, CheckoffRadius);

            // Draw waypoint ignore radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, waypointIgnoreRadius);
        }
    }
}
