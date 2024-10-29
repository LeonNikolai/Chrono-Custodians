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
        GameObject entrance = GenerateRoom(true);
        int iterations = 0;
        while (GeneratedRooms.Count < generationBudget)
        {
            iterations++;
            GameObject newRoom = GenerateRoom(false);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.T));
            if (newRoom != null)
            {
                iterations = 0;
                GeneratedRooms.Add(newRoom);
                RemoveNullEntryPoints();
            }
            if (iterations > 20)
            {
                break;
            }
            yield return null;
        }
        FillRemainingEntryPoints();
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

        }
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
            LGEntryPointController selectedPoint = entryPoints[Random.Range(0, entryPoints.Count)];
            LGEntryPointController roomEntry = null;
            roomEntry = roomController.AlignRoomToEntryPoint(selectedPoint);

            // Check if it is colliding
            if (roomEntry != null)
            {
                validRoomGenerated = true;
                RemoveEntryPoint(selectedPoint);
                Destroy(selectedPoint.gameObject);
                Destroy(roomEntry.gameObject);
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
}
