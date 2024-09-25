using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    public static Dictionary<int, Transform> AllSpawnPoints = new();
    public static List<Transform> GetShuffledSpawnPoints()
    {
        List<Transform> shuffledSpawnPoints = AllSpawnPoints.Values.ToList();
        for (int i = 0; i < shuffledSpawnPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledSpawnPoints.Count);
            Transform temp = shuffledSpawnPoints[i];
            shuffledSpawnPoints[i] = shuffledSpawnPoints[randomIndex];
            shuffledSpawnPoints[randomIndex] = temp;
        }
        return shuffledSpawnPoints;
    }

    void Awake()
    {
        AllSpawnPoints.Add(gameObject.GetInstanceID(), transform);
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        AllSpawnPoints.Remove(gameObject.GetInstanceID());
    }
}