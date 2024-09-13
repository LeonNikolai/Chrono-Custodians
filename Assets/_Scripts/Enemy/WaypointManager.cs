using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // There should only be one instance, so destroy duplicates
        }
    }

    public List<Transform> GetWaypoints()
    {
        return waypoints;
    }
}
