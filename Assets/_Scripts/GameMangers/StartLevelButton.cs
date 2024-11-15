using System;
using System.Collections;
using Unity.Mathematics;
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
    public bool Interactable => CoolDown.Value <= 0;
    public string InteractionMessage => SelectedLevel ? GameManager.instance.gameState.Value == GameManager.GameState.InLevel ? "Leave Level (buggy) ;)" : "Start Level" : "Select A Level";
    public string CantInteractMessage => $"Time Macine Cooling down {CoolDown.Value}s";
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
    NetworkVariable<float> CoolDown = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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
        StartCoroutine(CoolDownCoroutine());
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

    private IEnumerator CoolDownCoroutine()
    {
        CoolDown.Value = 10;
        float coolDown = 10;
        while (coolDown > 0)
        {
            coolDown -= Time.deltaTime;
            var difference = math.abs(CoolDown.Value - coolDown);
            if (difference > 1)
            {
                CoolDown.Value = Mathf.RoundToInt(coolDown);
            }
            yield return null;
        }
        CoolDown.Value = 0;
    }
}