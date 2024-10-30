using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LGManager : NetworkBehaviour
{
    public static LGManager Instance { get; private set; }

    [SerializeField] private Transform generationPoint;
    [SerializeField] private LGTheme theme;
    [SerializeField] private int generationBudget = 10;
    private int numberOfRooms = 0;
    [SerializeField] private float overlapThreshold = 1;
    [SerializeField] private float influenceRadius = 10;

    private List<GameObject> GeneratedRooms = new List<GameObject>();

    private List<LGEntryPointController> entryPoints = new List<LGEntryPointController>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        StartCoroutine(GenerateLevel());
    }

    private IEnumerator GenerateLevel()
    {
        float time = Time.time;
        GameObject entrance = GenerateRoom(true);
        int iterations = 0;
        while (GeneratedRooms.Count < generationBudget)
        {
            iterations++;
            GameObject newRoom = GenerateRoom(false);
            //yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.T));
            if (newRoom != null)
            {
                iterations = 0;
                GeneratedRooms.Add(newRoom);
            }
            if (iterations > 20)
            {
                break;
            }
            yield return null;
        }
        
        FillRemainingEntryPoints();
        Debug.Log("Level generation took " + (Time.time - time) + " seconds");
        // Its done
        NavmeshManager.Instance.BuildNavMesh();
    }

    public void AddEntryPoint(LGEntryPointController _entryPoint)
    {
        // Add entry point to list for generation
        entryPoints.Add(_entryPoint);
    }

    public void RemoveEntryPoint(LGEntryPointController _entryPoint)
    {
        if (entryPoints.Contains(_entryPoint))
        {
            entryPoints.Remove(_entryPoint);
        }
        else
        {
            Debug.Log("Entry point cannot be removed as it does not exist in this list");
        }
    }

    public void RemoveNullEntryPoints()
    {
        entryPoints.RemoveAll(entryPoint => entryPoint == null);
    }

    private void FillRemainingEntryPoints()
    {
        // Fill each entry point with a wall prefab
        foreach(LGEntryPointController _entryPoint in entryPoints)
        {
            FillEntryPoint(_entryPoint);
        }
        entryPoints.Clear();
    }

    private void FillEntryPoint(LGEntryPointController _entryPoint)
    {
        GameObject wall = Instantiate(theme.Wall);
        wall.transform.position = _entryPoint.transform.position;
        wall.transform.rotation = _entryPoint.transform.rotation;
        Destroy(_entryPoint.gameObject);
    }

    private GameObject GenerateRoom(bool isStartingEntrance)
    {
        GameObject room;
        bool validRoomGenerated = false;
        if (isStartingEntrance)
        {
            room = Instantiate(theme.Entrance);
            room.GetComponent<NetworkObject>().Spawn();
            room.transform.position = generationPoint.transform.position;
            room.GetComponent<LGRoom>().AddEntryPoints();
            GeneratedRooms.Add(room);
            return room;
        }

        int iterationCount = 0;
        // Keep trying until a valid room is generated
        while (!validRoomGenerated)
        {
            iterationCount++;
            room = Instantiate(theme.Rooms[Random.Range(0, theme.Rooms.Count)]);
            room.GetComponent<NetworkObject>().Spawn();
            LGRoom roomController = room.GetComponent<LGRoom>();

            // Align the rooms to entrypoint
            RemoveNullEntryPoints();
            LGEntryPointController selectedPoint = entryPoints[Random.Range(0, entryPoints.Count)];
            LGEntryPointController roomEntry = null;
            roomEntry = roomController.AlignRoomToEntryPoint(selectedPoint);

            // Check if it is colliding
            if (roomEntry != null && !IsRoomOverlappingNearby(roomController))
            {
                validRoomGenerated = true;
                RemoveEntryPoint(selectedPoint);
                Destroy(selectedPoint.gameObject);
                Destroy(roomEntry.gameObject);
                roomController.AddEntryPoints();
                return room;
            }
            else
            {
                roomController.GetComponent<NetworkObject>().Despawn();
            }
            if (iterationCount > 5)
            {
                Debug.Log("Can't spawn room");
                return null;
            }
        }

        return null;
    }

    private bool IsRoomOverlappingNearby(LGRoom newRoom)
    {
        // Use OverlapSphere to find nearby rooms within the influence radius
        Collider[] nearbyColliders = Physics.OverlapSphere(newRoom.transform.position, influenceRadius);

        foreach (Collider col in nearbyColliders)
        {
            LGRoom nearbyRoom = col.GetComponent<LGRoom>();
            if (nearbyRoom != null && nearbyRoom != newRoom && newRoom.IsOverlapping(nearbyRoom, overlapThreshold))
            {
                Debug.Log("Room overlap detected with a nearby room.");
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        foreach(var entryPoint in entryPoints)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(entryPoint.transform.position, 1f);
        }
    }
}
