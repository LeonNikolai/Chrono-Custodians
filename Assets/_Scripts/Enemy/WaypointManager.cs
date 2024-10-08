using System.Collections.Generic;
using UnityEngine;

public enum WaypointType
{
    Outside,
    Inside
}

[DefaultExecutionOrder(-2)]
public class WaypointManager : MonoBehaviour
{
    public static WaypointManager instance { get; private set; } = null;

    [SerializeField] private List<Transform> waypointsOutside = new List<Transform>();
    [SerializeField] private List<Transform> waypointsInside = new List<Transform>();

     private List<Vector3> outsideVectors = new List<Vector3>();
    [HideInInspector] private List<Vector3> insideVectors = new List<Vector3>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Start()
    {
        Debug.Log("WaypointManager is ready");
        if (instance == null || instance == this)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("There should only be one instance of WaypointManager");
            Destroy(gameObject); // There should only be one instance, so destroy duplicates
            return;
        }
    }

    private void UpdateWaypoints()
    {
        outsideVectors.Clear();
        for (int i = 0; i < waypointsOutside.Count; i++)
        {
            if (waypointsOutside[i] == null) continue;
            outsideVectors.Add(waypointsOutside[i].position);
        }

        insideVectors.Clear();
        for (int i = 0; i < waypointsInside.Count; i++)
        {
            if (waypointsInside[i] == null) continue;
            insideVectors.Add(waypointsInside[i].position);
        }
    }

    private void OnValidate()
    {
        UpdateWaypoints();
    }

    public List<Vector3> GetWaypoints(WaypointType type)
    {
        switch (type)
        {
            default: 
                return outsideVectors;

            case WaypointType.Outside:
                return outsideVectors;

            case WaypointType.Inside:
                return insideVectors;
        }
    }

    public void FindWaypoints()
    {
        WaypointController[] waypoints = FindObjectsByType<WaypointController>(FindObjectsSortMode.None);
        waypointsOutside.Clear();
        waypointsInside.Clear();

        foreach (var waypoint in waypoints)
        {
            if (waypoint.type == WaypointType.Outside)
            {
                waypointsOutside.Add(waypoint.transform);
            }
            else if (waypoint.type == WaypointType.Inside)
            {
                waypointsInside.Add(waypoint.transform);
            }
        }

        UpdateWaypoints();
    }
}
