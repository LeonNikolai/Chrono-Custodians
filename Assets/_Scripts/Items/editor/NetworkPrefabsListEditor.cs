using UnityEngine;

using Unity.Netcode;
using Unity.Netcode.Editor;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System;
using System.Threading.Tasks;

[CustomEditor(typeof(NetworkPrefabsList), true)]
[CanEditMultipleObjects]
public class NetworkPrefabsListExtensionEditor : NetworkPrefabsEditor
{
    NetworkPrefabsList prefablist;

    public override void OnInspectorGUI()
    {
        prefablist = (NetworkPrefabsList)target;

        EditorGUILayout.HelpBox("Refreshing (button) will clear all arrays, and search the project and add all unique instances for each.", MessageType.Info);
        bool didEdit = false;
        if (GUILayout.Button("Refresh"))
        {
            AddNetworkPrefabs();
            didEdit = true;
            // unselect editor
            Selection.SetActiveObjectWithContext(null, null);
        }
        base.OnInspectorGUI();

        if (didEdit)
        {
            EditorUtility.SetDirty(prefablist);
            AssetDatabase.SaveAssets();
        }
    }

    private void AddNetworkPrefabs()
    {
        while (prefablist.PrefabList.Count > 0)
        {
            prefablist.Remove(prefablist.PrefabList[0]);
        }
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        var prefabsToAdd = new List<GameObject>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                if (prefab.TryGetComponent<NetworkObject>(out _))
                {
                    prefabsToAdd.Add(prefab);
                }
            }
        }

        foreach (GameObject prefab in prefabsToAdd)
        {
            var networkPrefab = new NetworkPrefab { Prefab = prefab };
            prefablist.Add(networkPrefab);
        }


        Debug.Log($"Added {prefabsToAdd.Count} prefabs to the NetworkPrefabsList.");
    }
}