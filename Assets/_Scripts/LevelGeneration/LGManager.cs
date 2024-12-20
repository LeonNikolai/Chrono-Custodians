using System;
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
    private bool isDone = false;

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
    public override void OnDestroy()
    {
        GameManager.DestroyCurrentLevel -= DestroyLevel;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsHost)
        {
            GameManager.DestroyCurrentLevel += DestroyLevel;
            StartCoroutine(GenerateLevel());
        }
    }

    public HashSet<NetworkObject> SpawnedNetworkObjects = new HashSet<NetworkObject>();
    private void DestroyLevel()
    {
        foreach (var obj in SpawnedNetworkObjects)
        {
            obj?.Despawn(true);
        }
        SpawnedNetworkObjects.Clear();
    }

    private Transform currentEntrypoint;
    public static event Action OnLevelGenerated = delegate { };
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
        RemoveNullEntryPoints();
        FillRemainingEntryPoints();
        entryPoints.Clear();
        yield return new WaitUntil(() => isDone);
        Debug.Log("Level generation took " + (Time.time - time) + " seconds");
        // Its done
        NavmeshManager.Instance.BuildNavMesh();
        OnLevelGenerated?.Invoke();
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
        foreach (LGEntryPointController _entryPoint in entryPoints)
        {
            if (theme.Deadend == null)
            {
                FillEntryPoint(_entryPoint);
            }
            else
            {
                AddDeadEnd(_entryPoint);
            }
        }
        isDone = true;
        entryPoints.Clear();
    }

    private void FillEntryPoint(LGEntryPointController _entryPoint)
    {
        GameObject wall = Instantiate(theme.Wall);
        wall.transform.position = _entryPoint.transform.position;
        wall.transform.rotation = _entryPoint.transform.rotation;
        if (wall.TryGetComponent<NetworkObject>(out var wallobj))
        {
            wallobj.Spawn(true);
            SpawnedNetworkObjects.Add(wallobj);
        }
        Destroy(_entryPoint.gameObject);
    }

    private void AddDeadEnd(LGEntryPointController _entryPoint)
    {
        GameObject deadEnd = Instantiate(theme.Deadend);
        LGRoom deadEndRoom = deadEnd.GetComponent<LGRoom>();
        LGEntryPointController deadEndEntry = deadEndRoom.AlignRoomToEntryPoint(_entryPoint);
        if (IsRoomOverlappingNearby(deadEndRoom))
        {
            Debug.Log("Overlap!");
            Destroy(deadEnd.gameObject);
            if (Physics.Raycast(_entryPoint.transform.position + _entryPoint.transform.forward, Vector3.down, 5))
            {
                Destroy(_entryPoint);
                return;
            }
            else
            {
                FillEntryPoint(_entryPoint);
            }
        }
        else
        {
            Destroy(_entryPoint.gameObject);
            Destroy(deadEndEntry.gameObject);
            if (deadEnd.TryGetComponent<NetworkObject>(out var networkObject))
            {
                networkObject.Spawn(true);
                SpawnedNetworkObjects.Add(networkObject);
            }
        }

    }

    private GameObject GenerateRoom(bool isStartingEntrance)
    {
        GameObject room;
        bool validRoomGenerated = false;
        if (isStartingEntrance)
        {
            room = Instantiate(theme.Entrance);

            room.transform.position = generationPoint.transform.position + room.transform.position;
            if (room.TryGetComponent<NetworkObject>(out var entreanceRoomObject))
            {
                entreanceRoomObject.Spawn(true);
                SpawnedNetworkObjects.Add(entreanceRoomObject);
            }
            room.GetComponent<LGRoom>().AddEntryPoints();
            GeneratedRooms.Add(room);
            return room;
        }

        int iterationCount = 0;
        // Keep trying until a valid room is generated
        while (!validRoomGenerated)
        {
            iterationCount++;
            RemoveNullEntryPoints();
            LGEntryPointController selectedPoint = entryPoints[UnityEngine.Random.Range(0, entryPoints.Count)];
            RoomType previousRoom = selectedPoint.GetRoomType();
            switch (previousRoom)
            {
                default:
                    int randomChoice = UnityEngine.Random.Range(0, 2);
                    if (theme.Rooms == null || theme.Rooms.Count == 0)
                    {
                        randomChoice = 0;
                    }
                    if (randomChoice == 0)
                    {
                        room = Instantiate(theme.Corridors[UnityEngine.Random.Range(0, theme.Corridors.Count)]);
                    }
                    else
                    {
                        room = Instantiate(theme.Rooms[UnityEngine.Random.Range(0, theme.Rooms.Count)]);
                    }
                    break;

                case RoomType.Room:
                    room = Instantiate(theme.Corridors[UnityEngine.Random.Range(0, theme.Corridors.Count)]);
                    break;
            }
            LGRoom roomController = room.GetComponent<LGRoom>();

            // Align the rooms to entrypoint
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
                if (room.TryGetComponent<NetworkObject>(out var networkObject))
                {
                    networkObject.Spawn(true);
                    SpawnedNetworkObjects.Add(networkObject);
                }
                return room;
            }
            else
            {
                Destroy(roomController.gameObject);
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
        Gizmos.color = Color.red;
        if (currentEntrypoint == null) return;
        Gizmos.DrawSphere(currentEntrypoint.position, 1f);
    }
}
