using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(WaypointManager))]
public class WaypointManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        WaypointManager waypointManager = (WaypointManager)target;

        // Add a button to the inspector
        if (GUILayout.Button("Find Waypoints"))
        {
            waypointManager.FindWaypoints();
        }
    }
}
#endif