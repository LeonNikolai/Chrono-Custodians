using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(ItemIdProvider))]
public class ItemIdProviderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Refreshing (button) will clear all arrays, and search the project and add all unique instances for each.", MessageType.Info);
        if (GUILayout.Button("Refresh"))
        {
            ItemIdProvider provider = (ItemIdProvider)target;
            provider.FindAllItems();
            Debug.Log("Find All Items executed.");
        }

        GUILayout.Space(10);
        DrawDefaultInspector();
    }
}