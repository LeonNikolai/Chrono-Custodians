using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LGRoom))]
public class LGRoomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LGRoom lgRoom = (LGRoom)target;

        // Button to call SetRoomBounds method in LGRoom
        if (GUILayout.Button("Set Room Bounds"))
        {
            lgRoom.SetRoomBounds();
            EditorUtility.SetDirty(lgRoom); // Ensure changes are saved
        }
    }
}
