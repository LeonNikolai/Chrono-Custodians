using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Game : NetworkBehaviour
{
    public bool UseSaveSystem = false;
    public static string CurrentSaveSlot { get; private set; } = "SaveSlot1";
    public static float TimeStability
    {
        get => 75;
    }

    static GameSaveData _gameData;

    static bool _isLoading = false;
    public static bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (_isLoading == value) return;
            _isLoading = value;
            IsLoadingChanged?.Invoke(value);
        }
    }

    public static UnityEvent<bool> IsLoadingChanged = new UnityEvent<bool>();

    private void Update()
    {
        IsLoading = LevelManager.IsLoading;
    }
}