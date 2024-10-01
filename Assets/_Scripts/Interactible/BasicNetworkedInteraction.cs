using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class BasicNetworkedInteraction : NetworkBehaviour, IHighlightable, IInteractable
{
    [Header("Networked Events")]
    [SerializeField] UnityEvent _onInteractServer = new UnityEvent();
    [SerializeField] UnityEvent _onInteractOtherClients = new UnityEvent();
    [Header("Local Interactor Events")]
    [SerializeField] UnityEvent _onInteractOwnClientLocal = new UnityEvent();
    [Header("Local Interactor Highlight Events")]
    [SerializeField] UnityEvent _onHighlightEnterLocal = new UnityEvent();
    [SerializeField] UnityEvent _onHighlightExitLocal = new UnityEvent();

    public bool Interactible => true;

    public void HightlightEnter()
    {
        _onHighlightEnterLocal?.Invoke();
    }

    public void HightlightExit()
    {
        _onHighlightExitLocal?.Invoke();
    }

    public void HightlightUpdate()
    {

    }

    public void Interact(Player player)
    {

        _onInteractOwnClientLocal?.Invoke();
        InteractRpc();
    }

    [Rpc(SendTo.NotMe)]
    private void InteractRpc()
    {
        if (IsServer) _onInteractServer?.Invoke();
        _onInteractOtherClients?.Invoke();
    }
}