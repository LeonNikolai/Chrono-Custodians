using NUnit.Framework;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavmeshManager : MonoBehaviour
{
    public static NavmeshManager Instance;

    [SerializeField] private LayerMask navmeshBuildLayer;

    [SerializeField] private List<NavmeshAgentType> agentTypesToBuild; // Set this in the Inspector

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else if (Instance != null && Instance != this) 
        {
            Destroy(this);
        }
    }

    public void BuildNavMesh()
    {
        foreach(var agent in agentTypesToBuild)
        {
            GameObject go = Instantiate(new GameObject(), transform);
            NavMeshSurface navMesh = go.AddComponent<NavMeshSurface>();
            NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(agent.agentTypeID);
            navMesh.layerMask = navmeshBuildLayer;
            navMesh.agentTypeID = settings.agentTypeID;
            navMesh.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            navMesh.BuildNavMesh();
        }
    }
}
