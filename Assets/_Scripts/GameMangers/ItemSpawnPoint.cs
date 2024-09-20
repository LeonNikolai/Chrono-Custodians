using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    public static HashSet<ItemSpawnPoint> AllSpawnPoints = new HashSet<ItemSpawnPoint>();
    public static List<ItemSpawnPoint> GetShuffledSpawnPoints()
    {
        List<ItemSpawnPoint> shuffledSpawnPoints = new List<ItemSpawnPoint>(AllSpawnPoints);
        for (int i = 0; i < shuffledSpawnPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledSpawnPoints.Count);
            ItemSpawnPoint temp = shuffledSpawnPoints[i];
            shuffledSpawnPoints[i] = shuffledSpawnPoints[randomIndex];
            shuffledSpawnPoints[randomIndex] = temp;
        }
        return shuffledSpawnPoints;
    }

    void Awake()
    {
        AllSpawnPoints.Add(this);
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        AllSpawnPoints.Remove(this);
    }
}