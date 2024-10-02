using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
[DefaultExecutionOrder(-2)]
public class WaypointManager : MonoBehaviour
{
    public static WaypointManager instance { get; private set; } = null;

    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [HideInInspector] public List<Vector3> waypointPosition = new List<Vector3>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        UpdatePoints();
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
        UpdatePoints();
    }

    private void UpdatePoints()
    {
        waypointPosition.Clear();
        for (int i = 0; i < waypoints.Count; i++)
        {
            if(waypoints[i] == null) continue;
            waypointPosition.Add(waypoints[i].position);
        }
    }
}
