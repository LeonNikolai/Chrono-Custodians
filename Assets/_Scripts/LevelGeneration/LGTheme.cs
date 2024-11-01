using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LGTheme", menuName = "Level Generation/Theme", order = 1)]
public class LGTheme : ScriptableObject
{
    [SerializeField] private List<GameObject> corridorPrefabs;
    [SerializeField] private List<GameObject> roomPrefabs;
    [SerializeField] private GameObject entrancePrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject deadEndPrefab;

    // Optionally, you can add methods to interact with the list
    public List<GameObject> Corridors => corridorPrefabs;
    public List<GameObject> Rooms => roomPrefabs;
    public GameObject Entrance => entrancePrefab;
    public GameObject Wall => wallPrefab;
    public GameObject Deadend => deadEndPrefab;
}
