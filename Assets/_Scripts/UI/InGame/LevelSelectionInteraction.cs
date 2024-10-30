using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class LevelSelectionInteraction : NetworkBehaviour, IInteractable, IInteractionMessage, IScanable
{

    public static event Action<LevelScene> OnSelectLevelServer = delegate { };
    [SerializeField] private LevelScene[] _levelScenes;
    LevelScene levelScene => _levelScenes != null && _levelScenes.Length >= 1 ? _levelScenes[0] : null;
    LevelScene randomLevel
    {
        get
        {
            if (_levelScenes == null || _levelScenes.Length == 0) return null;
            if (_levelScenes.Length == 1) return _levelScenes[0];
            return _levelScenes[UnityEngine.Random.Range(0, _levelScenes.Length)];
        }
    }
    [SerializeField] private bool _interactible = true;
    [SerializeField] private bool _onlyHost = false;
    [SerializeField] private bool _onlyIfNoLevelLoaded = false;

    public bool Interactable
    {
        get
        {
            if (levelScene == null) return false;
            if (_onlyIfNoLevelLoaded) return LevelManager.LoadedScene == null;
            if (_onlyHost) return IsHost;
            return _interactible;
        }
    }

    public string InteractionMessage => "Interact (E) to travel to " + levelScene.LevelName;

    public string CantInteractMessage
    {
        get
        {
            if (levelScene == null) return "This machine is not set up properly";
            if (_onlyIfNoLevelLoaded) return "The time machine is already in use";
            if (_onlyHost) return "Only the host can interact with this";
            return "You are not authorized to time travel here yet";
        }
    }

    public string ScanTitle => "Time machine for Dummies";

    public string ScanResult
    {
        get
        {
            if (levelScene == null) return "With a single interaction, you can make the machine go somewhere.\n\nThis machine is not connected to the TWW (time wide web)";
            return "With a single interaction, you can make the machine go somewhere.\n\nThis machine is connected to the TWW (time wide web) and can be used to time travel to " + levelScene.LevelName;
        }
    }

    public void Interact(Player player)
    {
        if (!Interactable) return;
        StartCoroutine(ScanCoroutine());
        if (IsHost || IsServer)
        {
            LoadLevel();
        }
        else
        {
            if (_onlyHost) return;
            LoadLevelServerRpc();
        }
    }
    [Rpc(SendTo.Server)]
    private void LoadLevelServerRpc()
    {
        LoadLevel();
    }

    public void LoadLevel()
    {
        OnSelectLevelServer.Invoke(randomLevel);
    }

    public void OnScan(Player player)
    {
        StartCoroutine(ScanCoroutine());
    }

    private IEnumerator ScanCoroutine()
    {
        float multiplier = 1f;
        while (true)
        {
            multiplier += Time.deltaTime;
            if (multiplier > 1.1f)
            {
                multiplier = 1.1f;
                break;
            }
            transform.localScale = Vector3.one * multiplier;
            yield return null;
        }
        while (true)
        {
            multiplier -= Time.deltaTime;
            if (multiplier < 1f)
            {
                multiplier = 1f;
                break;
            }
            transform.localScale = Vector3.one * multiplier;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}