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

        while (GeneratedRooms.Count < generationBudget)
        {
            GameObject newRoom = GenerateRoom(false);
            if (newRoom != null)
            {
                GeneratedRooms.Add(newRoom);
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
        // Keep trying until a valid room is generated
        while (!validRoomGenerated)
        {
            room = Instantiate(theme.Rooms[Random.Range(0, theme.Rooms.Count)]);
            LGRoom roomController = room.GetComponent<LGRoom>();

            // Align the rooms to entrypoint
            roomController.AlignRoomToEntryPoint(entryPoints[Random.Range(0, entryPoints.Count)]);

            // Check if it is colliding
            if (!roomController.isColliding())
            {
                validRoomGenerated = true;
                return room;
            }
            else
            {
                Destroy(room);  // Destroy the invalid room
            }
        }

        return null;
    }
}
