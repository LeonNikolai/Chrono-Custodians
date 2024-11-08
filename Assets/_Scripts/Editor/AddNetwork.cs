using UnityEditor;
using UnityEngine;
using Unity.Netcode; // Ensure this is correct for your version of Netcode for GameObjects
using System.Collections.Generic;

public class NetworkPrefabsEditor : EditorWindow
{
    private NetworkPrefabsList prefablist;
    private List<GameObject> prefabsToAdd;

    [MenuItem("Tools/Network Prefabs Manager")]
    public static void ShowWindow()
    {
        GetWindow<NetworkPrefabsEditor>("Network Prefabs Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("Network Prefabs List Manager", EditorStyles.boldLabel);
        prefablist = (NetworkPrefabsList)EditorGUILayout.ObjectField("Network Manager", prefablist, typeof(NetworkPrefabsList), true);

        if (prefablist == null)
        {
            EditorGUILayout.HelpBox("Please assign a NetworkManager with a NetworkPrefabsList.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Add NetworkObjects to NetworkPrefabsList"))
        {
            AddNetworkPrefabs();
        }
    }

    private void AddNetworkPrefabs()
    {
        while (prefablist.PrefabList.Count > 0)
        {
            prefablist.Remove(prefablist.PrefabList[0]);
        }
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        prefabsToAdd = new List<GameObject>();

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

        EditorUtility.SetDirty(prefablist);
        AssetDatabase.SaveAssets();

        Debug.Log($"Added {prefabsToAdd.Count} prefabs to the NetworkPrefabsList.");
    }
}