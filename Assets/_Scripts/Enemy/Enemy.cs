using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;

public abstract class Enemy : NetworkBehaviour
{
    public enum EnemyState
    {
        Roaming,
        Searching
    }
    [Header("Enemy Base")]
    // Base Statistics
    public bool DrawAttackGizmos = false;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected float attackRange;
    [SerializeField] protected int attackDamage;
    [SerializeField] protected float attackCooldown;
    [SerializeField] protected float moveSpeed;
    [SerializeField] public int tokenCost = 2; // This is how difficult the enemy is. Higher means it's more difficult to deal with. Try to stay within a scale of 1-5

    protected NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    [Header("Enemy Base AI Behavior")]
    public bool DrawWaypointGizmos = false;

    // AI Pathfinding Behavior Variables
    protected NavMeshAgent agent;
    [SerializeField] protected bool isRoaming = true;
    [SerializeField] private Vector3 targetWaypoint;
    [SerializeField] private float CheckoffRadius = 5f;
    [SerializeField] private float waypointIgnoreRadius = 15f;
    [SerializeField] private WaypointType insideOrOutside;
    public List<Vector3> waypoints = new List<Vector3>();
    public Queue<Vector3> visitedWaypoints = new Queue<Vector3>();
    [HideInInspector] public GameObject player;
    [Space]
    [SerializeField] private int searchingAmount = 7;
    private bool isSearching = false;



    [Header("Debug")]
    [SerializeField] private bool debug = false;
    

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
            StartRoaming();
        }
    }

    private void Update()
    {
        if(!IsSpawned) return;
        if (!IsServer) return;

        if (!isRoaming) return;
        // Checks if the enemy is roaming and the distance between it and the target waypoint is within the checkoff radius
        float sqrDistance = Vector3.SqrMagnitude(transform.position - targetWaypoint);
        if (sqrDistance < CheckoffRadius * CheckoffRadius) WaypointReached();
    }

    private void GetWaypoints()
    {
        waypoints = WaypointManager.instance.GetWaypoints(insideOrOutside);
        // Enemy should find a waypoint manager and get a list of waypoints that are active.
    }

    public void SetWaypoints(List<Vector3> _waypoints)
    {
        waypoints = _waypoints;
    }

    protected void WaypointReached()
    {
        // Adds the reached waypoint to a list and selects a new waypoint to go to.
        waypoints.Remove(targetWaypoint);
        visitedWaypoints.Enqueue(targetWaypoint);

    }

    private void SelectNewWaypoint()
    {
        Vector3 newTarget;
        if (waypoints.Count <= 0) return;
        do
        {
            newTarget = waypoints[Random.Range(0, waypoints.Count)];
        } while (Vector3.SqrMagnitude(transform.position - newTarget) < waypointIgnoreRadius * waypointIgnoreRadius);

        if (visitedWaypoints.Count > 10)
        {
            Vector3 tempWaypoint = visitedWaypoints.Dequeue();
            waypoints.Add(tempWaypoint);
        }
        targetWaypoint = newTarget;
    } 

    private void SelectWaypointNearby()
    {
        Vector3 newTarget;
        if (waypoints.Count <= 0) return;
        do
        {
            newTarget = waypoints[Random.Range(0, waypoints.Count)];
        } while (Vector3.SqrMagnitude(transform.position - newTarget) > waypointIgnoreRadius * waypointIgnoreRadius);

        if (visitedWaypoints.Count > 10)
        {
            Vector3 tempWaypoint = visitedWaypoints.Dequeue();
            waypoints.Add(tempWaypoint);
        }
        targetWaypoint = newTarget;
    }    

    private void MoveToWaypoint()
    {
        agent.SetDestination(targetWaypoint);
    }

    public virtual void StopRoaming()
    {
        isRoaming = false;
        agent.SetDestination(transform.position);
    }

    public virtual void StartRoaming()
    {
        isRoaming = true;
        StartCoroutine(Roaming());
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

        if (targetWaypoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetWaypoint, 1f);
    }

    public virtual void StareAtPlayer()
    {
        if (!player) return;

        // Calculate the direction to the player
        Vector3 direction = player.transform.position - transform.position;

        // Create the rotation we need to look at the target
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Get the existing rotation (to lock specific axes)
        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 targetEuler = lookRotation.eulerAngles;

        // Lock specific axes by overriding the values you want to lock
        targetEuler.x = currentEuler.x; // Lock the X-axis
        // targetEuler.y = currentEuler.y; // Lock the Y-axis
        targetEuler.z = currentEuler.z; // Lock the Z-axis

        // Apply the modified rotation
        Quaternion finalRotation = Quaternion.Euler(targetEuler);

        // Smoothly rotate towards the target while keeping the locked axes unchanged
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.deltaTime * 40);
    }


    public virtual IEnumerator Roaming()
    {
        isRoaming = true;
        while (isRoaming)
        {
            SelectNewWaypoint();
            agent.SetDestination(targetWaypoint);
            yield return new WaitUntil(() => Vector3.SqrMagnitude(transform.position - targetWaypoint) <= CheckoffRadius * CheckoffRadius);
            waypoints.Remove(targetWaypoint);
            visitedWaypoints.Enqueue(targetWaypoint);
        }
        // If there's any logic after roaming, it comes after base, though typically roaming will continue until something else stops it.
    }

    public virtual IEnumerator Searching()
    {
        Debug.Log("Start Searching");
        isSearching = true;
        int waypointsSearched = searchingAmount;
        int counter = 0;

        while (waypointsSearched > 0)
        {
            counter++;
            yield return null;
            SelectWaypointNearby();
            agent.SetDestination(targetWaypoint);
            Debug.Log("Moving to Search Point");
            yield return new WaitUntil(() => Vector3.SqrMagnitude(transform.position - targetWaypoint) <= CheckoffRadius * CheckoffRadius);
            Debug.Log("Search point reached");
            waypoints.Remove(targetWaypoint);
            visitedWaypoints.Enqueue(targetWaypoint);
            waypointsSearched -= 1;
            if (waypointsSearched == 0)
            {
                Debug.Log("Stop Searching");
                isSearching = false;
            }
            if (counter >= 50)
            {
                Debug.Log("Exceeding Run amount");
                yield break;
            }
        }
        // Logic after searching comes after Base
    }

    public virtual void ResetEnemyToNormal()
    {
        isSearching = false;
        isRoaming = false;
        agent.speed = moveSpeed;

    }
}
