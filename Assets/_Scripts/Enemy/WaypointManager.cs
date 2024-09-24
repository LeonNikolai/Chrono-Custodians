using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager instance { get; private set; }

    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [HideInInspector] public List<Vector3> waypointPosition = new List<Vector3>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // There should only be one instance, so destroy duplicates
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Start()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            waypointPosition.Add(waypoints[i].position);
        }
    }
}
