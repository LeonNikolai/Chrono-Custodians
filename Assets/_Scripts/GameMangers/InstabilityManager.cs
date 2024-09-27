using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InstabilityManager : NetworkBehaviour
{
    public static InstabilityManager instance;

    [SerializeField] private float curInstability; // Current instability
    [SerializeField] private float maxInstability; // Max instability before game over.


    [SerializeField] private float objectInstability; // The instability one object causes
    [SerializeField] private int totalNumberOfObjects; // Number of objects in the timeline.
    private Dictionary<string, int> levelInstability; // Number of objects in a level

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        base.OnNetworkSpawn();
        if (instance == null)
        {
            instance = this;
        } else if (instance != null || instance != this)
        {
            Destroy(this);
        }
    }

    // Update the level's instability. This should happen at the end of a level/start of level selection
    public void UpdateLevelInstability(string level, int temporalObject)
    {
        if (!IsServer) return;
        if (levelInstability.ContainsKey(level))
        {
            levelInstability[level] += temporalObject;
        }
    }

    public int GetLevelObjectNumber(string levelName)
    {
        if (!IsServer) return 0;
        levelInstability.TryGetValue(levelName, out int levelObjects);
        return levelObjects;
    }

    private void UpdateTotalInstability()
    {
        if (!IsServer) return;
        if (levelInstability.Count == 0) return;
        totalNumberOfObjects = 0;
        foreach(var level in levelInstability)
        {
            totalNumberOfObjects += level.Value;
        }
    }

}
