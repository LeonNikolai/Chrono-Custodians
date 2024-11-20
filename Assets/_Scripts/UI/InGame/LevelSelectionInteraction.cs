using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class LevelSelectionInteraction : NetworkBehaviour, IInteractable, IInteractionMessage, IScanable
{
    public UnityEvent<int> ThisLevelSelected = new UnityEvent<int>();

    [SerializeField] private LevelScene _levelScenes;
    LevelScene levelScene => _levelScenes != null ? _levelScenes : null;
    [SerializeField, Tooltip("-1 == Random, other = index % scene.length")] private int _sceneIndex = -1;
    [SerializeField] private bool _interactible = true;
    [SerializeField] private bool _onlyHost = false;
    [SerializeField] private bool _onlyIfNoLevelLoaded = false;
    Vector3 originalScale;
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
    void Awake()
    {
        originalScale = transform.localScale;
    }

    public override void OnNetworkSpawn()
    {
        transform.localScale = originalScale;
        base.OnNetworkSpawn();
        StartLevelButton.OnSelectLevel += OnLevelSelect;
        LevelManager.instance.OnLevelLoaded.AddListener(OnLevelLoaded);
        OnLevelSelect(null, -1);
    }

    private void OnLevelLoaded(LevelScene level)
    {
        OnLevelSelect(level, -1);
    }

    public override void OnDestroy()
    {
        StartLevelButton.OnSelectLevel -= OnLevelSelect;
        base.OnDestroy();
    }

    const int UnSelectedLightIndex = 0;
    const int SelectedLightIndex = 1;
    const int FullStabilityIndex = 2;
    const int LowStabilityLightIndex = 3;

    private void OnLevelSelect(LevelScene scene, int index)
    {
        bool rightIndex = _sceneIndex == -1 || _sceneIndex == index;
        bool rightScene = levelScene == scene;

        float stability = _levelScenes != null ? Stability.GetLevelStability(_levelScenes).Stability : 0;
        bool lowStability = stability < 50;
        bool fullStability = stability == 100;

        if (rightIndex && rightScene) ThisLevelSelected.Invoke(SelectedLightIndex);
        else if (fullStability) ThisLevelSelected.Invoke(FullStabilityIndex);
        else if (lowStability) ThisLevelSelected.Invoke(LowStabilityLightIndex);
        else ThisLevelSelected.Invoke(UnSelectedLightIndex);
    }

    public void Interact(Player player)
    {
        if (!Interactable) return;
        StartCoroutine(ScanCoroutine());
        if (IsHost || IsServer)
        {
            SelectLevel();
        }
        else
        {
            if (_onlyHost) return;
            SelectLevelServerRpc();
        }
    }
    [Rpc(SendTo.Server)]
    private void SelectLevelServerRpc()
    {
        SelectLevel();
    }

    public void SelectLevel()
    {
        StartLevelButton.SelectLevel(levelScene, _sceneIndex);
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
            transform.localScale = originalScale * multiplier;
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
            transform.localScale = originalScale * multiplier;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}