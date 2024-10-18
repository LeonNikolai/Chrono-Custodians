using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class ItemScannerPointPath : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _followTrail;

    Vector3[] _path;
    public void Enabled(bool enabled)
    {
        _lineRenderer.enabled = enabled;
    }
    public void SetPath(Vector3[] path)
    {
        _lineRenderer.positionCount = path.Length;
        _path = path;
        _lineRenderer.SetPositions(path);
    }
    float pathLength = 0;


    public void SetPath(Vector3 end)
    {
        NavMeshPath path = new NavMeshPath();
        NavMeshHit hit;
        Vector3 pathEnd = end;
        Vector3 pathStart = transform.position;
        if (NavMesh.SamplePosition(pathStart, out hit, 3.0f, NavMesh.AllAreas))
        {
            pathStart = hit.position;
        }
        else if (NavMesh.FindClosestEdge(pathStart, out hit, NavMesh.AllAreas))
        {
            pathStart = hit.position;
        }

        if (NavMesh.SamplePosition(pathEnd, out hit, 3.0f, NavMesh.AllAreas))
        {
            pathEnd = hit.position;
        }
        else if (NavMesh.FindClosestEdge(end, out hit, NavMesh.AllAreas))
        {
            pathEnd = hit.position;
        }
        if (!NavMesh.CalculatePath(pathStart, pathEnd, NavMesh.AllAreas, path))
        {
            SetPath(new Vector3[] { transform.position, end });
            pathLength = Vector3.Distance(transform.position, end);
            return;
        }
        Vector3[] corners = path.corners;
        pathLength = 0;
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = new Vector3(corners[i].x, corners[i].y + 1f, corners[i].z);
            pathLength += Vector3.Distance(corners[i], corners[Mathf.Clamp(i + 1, 0, corners.Length - 1)]);
        }

        if (corners.Length < 2)
        {
            SetPath(new Vector3[] { transform.position, end });
            pathLength = Vector3.Distance(transform.position, end);
            return;
        }
        corners[path.corners.Length - 1] = end;
        corners[0] = transform.position;
        SetPath(corners);
        return;
    }
}
