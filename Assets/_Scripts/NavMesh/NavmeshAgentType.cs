using UnityEngine;

[CreateAssetMenu(menuName = "NavMesh/AgentType")]
public class NavmeshAgentType : ScriptableObject
{
    public string agentTypeName;
    public int agentTypeID;
}
