using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LGTheme", menuName = "Level Generation/Theme", order = 1)]
public class LGTheme : ScriptableObject
{
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private GameObject entrancePrefab;
    [SerializeField] private GameObject wallPrefab;

    // Optionally, you can add methods to interact with the list
    public List<GameObject> Rooms => prefabs;
    public GameObject Entrance => entrancePrefab;
    public GameObject Wall => wallPrefab;
}
