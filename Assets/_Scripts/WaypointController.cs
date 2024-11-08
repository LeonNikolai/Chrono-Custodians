
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
public enum WaypointType
{
    Outside,
    Inside
}
public class WaypointController : MonoBehaviour
{
    [SerializeField] public WaypointType type;

    static Dictionary<WaypointType, List<WaypointController>> WaypointControllers = new();
    static void AddWaypointController(WaypointController waypointController)
    {
        if (!WaypointControllers.ContainsKey(waypointController.type))
        {
            WaypointControllers[waypointController.type] = new List<WaypointController>();
        }
        WaypointControllers[waypointController.type].Add(waypointController);
        IsDirty[waypointController.type] = true;
    }
    static void RemoveWaypointController(WaypointController waypointController)
    {
        if (!WaypointControllers.ContainsKey(waypointController.type)) return;
        WaypointControllers[waypointController.type].Remove(waypointController);
        if (WaypointControllers[waypointController.type].Count == 0)
        {
            WaypointControllers.Remove(waypointController.type);
        }
        IsDirty[waypointController.type] = true;
    }
    static Dictionary<WaypointType, bool> IsDirty = new Dictionary<WaypointType, bool>();

    public static Dictionary<WaypointType, List<Vector3>> Waypoints = new();

    public static List<Vector3> GetWaypoint(WaypointType type)
    {
        if (IsDirty[type])
        {
            UpdateType(type);
        }
        return Waypoints[type];
    }

    private static void UpdateType(WaypointType type)
    {
        IsDirty[type] = false;
        if (!Waypoints.ContainsKey(type))
        {
            Waypoints[type] = new List<Vector3>();
        }
        Waypoints[type].Clear();
        foreach (var waypointController in WaypointControllers.Values)
        {
            foreach (var waypoint in waypointController)
            {
                Waypoints[type].Add(waypoint.transform.position);
            }
        }
    }

    void OnEnable()
    {
        AddWaypointController(this);
    }
    void OnDisable()
    {
        RemoveWaypointController(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    public void SnapToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, Mathf.Infinity, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        else if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        else
        {
            Debug.LogWarning("Could not snap to NavMesh");
        }
    }

    public void SnapToGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f))
        {
            transform.position = hit.point;
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(WaypointController))]
[CanEditMultipleObjects]
public class WaypointControllerEditor : Editor
{
    private void OnSceneGUI()
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 150, 100));

        if (GUILayout.Button("Snap All to NavMesh"))
        {
            SnapSelectedToNavMesh();
        }
        if (GUILayout.Button("Snap All to Ground"))
        {
            SnapSelectedToGround();
        }

        GUILayout.EndArea();

        Handles.EndGUI();
    }

    private void SnapSelectedToGround()
    {
        foreach (var obj in Selection.gameObjects)
        {
            WaypointController itemSpawnPoint = obj.GetComponent<WaypointController>();
            if (itemSpawnPoint != null)
            {
                itemSpawnPoint.SnapToGround();
            }
        }
    }

    private void SnapSelectedToNavMesh()
    {
        foreach (var obj in Selection.gameObjects)
        {
            WaypointController itemSpawnPoint = obj.GetComponent<WaypointController>();
            if (itemSpawnPoint != null)
            {
                itemSpawnPoint.SnapToNavMesh();
            }
        }
    }
}
#endif