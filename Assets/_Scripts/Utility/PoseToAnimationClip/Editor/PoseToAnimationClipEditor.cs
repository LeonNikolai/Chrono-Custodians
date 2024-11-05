using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PoseToAnimationClip))]
public class PoseToAnimationClipEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PoseToAnimationClip poseRecorder = (PoseToAnimationClip)target;
        if (GUILayout.Button("Create Animation Clip"))
        {
            poseRecorder.CreateAnimationClip();
        }
    }
}
