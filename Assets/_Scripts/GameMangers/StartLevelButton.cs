using System;
using Unity.Netcode;
using UnityEngine;

public class StartLevelButton : NetworkBehaviour, IInteractable, IInteractionMessage
{
    public static event Action<LevelScene, int> OnSelectLevel = delegate { };
    static event Action<LevelScene, int> selectLevelEvent = delegate { };
    public static void SelectLevel(LevelScene scene, int index)
    {
        selectLevelEvent?.Invoke(scene, index);
    }
    public bool Interactable => true;
    public string InteractionMessage => SelectedLevel ? GameManager.instance.gameState.Value == GameManager.GameState.InLevel ? "Leave Level (buggy) ;)" : "Start Level" : "Select A Level";
    public string CantInteractMessage => "";
    LevelScene selectedLevel;
    LevelScene SelectedLevel
    {
        get => selectedLevel;
        set
        {
            selectedLevel = value;
            if (IsHost)
            {
                SelectedSceneID.Value = LevelManager.GetSceneID(value);
            }
            OnSelectLevel?.Invoke(value, SelectedIndex.Value);
        }
    }
    NetworkVariable<int> SelectedSceneID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<int> SelectedIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private void Start()
    {
        selectLevelEvent += SelectALevel;
        SelectedSceneID.OnValueChanged += NewLevelId;
        SelectedIndex.OnValueChanged += NewIndex;
    }

    public override void OnDestroy()
    {
        selectLevelEvent -= SelectALevel;
        SelectedSceneID.OnValueChanged -= NewLevelId;
        SelectedIndex.OnValueChanged -= NewIndex;
        base.OnDestroy();
    }

    private void NewIndex(int previousValue, int newValue)
    {
        SelectedLevel = LevelManager.GetSceneById(SelectedSceneID.Value);
    }

    private void NewLevelId(int previousValue, int newValue)
    {
        SelectedLevel = LevelManager.GetSceneById(newValue);
    }

    private void SelectALevel(LevelScene scene, int newIndex)
    {
        SelectedIndex.Value = newIndex;
        SelectedLevel = scene;
    }

    public void Interact(Player player)
    {
        InteractRpc();
    }

    [Rpc(SendTo.Server)]
    public void InteractRpc()
    {
        if (LevelManager.LoadedScene == null)
        {
            if (SelectedLevel == null)
            {
                Debug.Log("Select a level before starting");
                return;
            }
            LevelScene levelToStart = SelectedLevel;
            GameManager.instance.LevelStart(levelToStart, SelectedIndex.Value);
            return;
        }

        LevelScene level = LevelManager.LoadedScene;
        GameManager.instance.LevelEnd(level);
        SelectedSceneID.Value = -1;
    }
}