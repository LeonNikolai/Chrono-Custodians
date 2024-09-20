using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
{
    public bool UseSaveSystem = false;
    public static string CurrentSaveSlot { get; private set; } = "SaveSlot1";
    public static float TimeStability
    {
        get => 75;
    }

    static GameSaveData _gameData;
    private void Awake()
    {

    }
}