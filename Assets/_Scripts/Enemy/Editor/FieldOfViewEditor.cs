using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fieldOfView = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fieldOfView.head.position, Vector3.up, Vector3.forward, 360, fieldOfView.radius);

        Vector3 viewAngle01 = DirectionFromAngle(fieldOfView.head.eulerAngles.y, -fieldOfView.angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(fieldOfView.head.eulerAngles.y, fieldOfView.angle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(fieldOfView.head.position, fieldOfView.head.position + viewAngle01 * fieldOfView.radius);
        Handles.DrawLine(fieldOfView.head.position, fieldOfView.head.position + viewAngle02 * fieldOfView.radius);

        if (fieldOfView.canSeeTarget)
        {
            Handles.color = Color.green;
            Handles.DrawLine(fieldOfView.head.position, fieldOfView.curtarget.transform.position);
        }
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));

    }
}
