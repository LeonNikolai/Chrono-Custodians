using System;
using Unity.Netcode;
using UnityEngine;

public class StartLevelButton : NetworkBehaviour, IInteractable, IInteractionMessage
{
    public bool Interactable => true;
    public string InteractionMessage => "";

    public string CantInteractMessage => "";
    LevelScene SelectedLevel;
    private void Start()
    {
        LevelSelectionInteraction.OnSelectLevelServer += OnLevelSelect;
    }

    private void OnLevelSelect(LevelScene scene)
    {
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
            GameManager.instance.LevelStart(levelToStart);
            SelectedLevel.LoadScene();
            return;
        }
        else
        {
            LevelScene level = LevelManager.LoadedScene;
            GameManager.instance.LevelEnd(level);
        }
    }
}