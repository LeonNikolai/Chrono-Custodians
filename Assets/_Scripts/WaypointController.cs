
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WaypointController : MonoBehaviour
{
    [SerializeField] public WaypointType type;

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