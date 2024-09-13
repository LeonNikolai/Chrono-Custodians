using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class BasicNetworkedInteraction : NetworkBehaviour, IHighlightable, IInteractable
{
    [SerializeField] UnityEvent _onInteractServer = new UnityEvent();
    [SerializeField] UnityEvent _onInteractClient = new UnityEvent();
    [SerializeField] UnityEvent _onHighlightEnter = new UnityEvent();
    [SerializeField] UnityEvent _onHighlightExit = new UnityEvent();
    public void HightlightEnter()
    {
        _onHighlightEnter?.Invoke();
    }

    public void HightlightExit()
    {
        _onHighlightExit?.Invoke();
    }

    public void HightlightUpdate()
    {

    }

    public void Interact(PlayerMovement player)
    {

        _onInteractClient?.Invoke();
        InteractRpc();
    }

    [Rpc(SendTo.Server)]
    private void InteractRpc()
    {
        _onInteractServer?.Invoke();
    }
}