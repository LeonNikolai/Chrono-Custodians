using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
# if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(-2)]
public class ItemSpawnPoint : MonoBehaviour
{
    public static Dictionary<int, Transform> AllSpawnPoints = new();
    public static List<Transform> GetShuffledSpawnPoints()
    {
        List<Transform> shuffledSpawnPoints = AllSpawnPoints.Values.ToList();
        for (int i = 0; i < shuffledSpawnPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledSpawnPoints.Count);
            Transform temp = shuffledSpawnPoints[i];
            shuffledSpawnPoints[i] = shuffledSpawnPoints[randomIndex];
            shuffledSpawnPoints[randomIndex] = temp;
        }
        return shuffledSpawnPoints;
    }

    void Awake()
    {
        AllSpawnPoints.Add(gameObject.GetInstanceID(), transform);
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        AllSpawnPoints.Remove(gameObject.GetInstanceID());
    }

    public void SnapToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
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
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ItemSpawnPoint))][CanEditMultipleObjects] 
public class ItemSpawnPointEditor : Editor
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

        // End the GUI block
        Handles.EndGUI();
    }

    private void SnapSelectedToGround()
    {
        foreach (var obj in Selection.gameObjects)
        {
            ItemSpawnPoint itemSpawnPoint = obj.GetComponent<ItemSpawnPoint>();
            if (itemSpawnPoint != null)
            {
                itemSpawnPoint.SnapToGround();
            }
        }
    }

    // Snap all selected ItemSpawnPoint objects to NavMesh
    private void SnapSelectedToNavMesh()
    {
        // Get all selected GameObjects in the scene
        foreach (var obj in Selection.gameObjects)
        {
            // Check if the GameObject has an ItemSpawnPoint component
            ItemSpawnPoint itemSpawnPoint = obj.GetComponent<ItemSpawnPoint>();
            if (itemSpawnPoint != null)
            {
                // Call the SnapToNavMesh method on each selected ItemSpawnPoint
                itemSpawnPoint.SnapToNavMesh();
            }
        }
    }
}
#endif