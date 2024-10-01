using UnityEngine;
using UnityEngine.Events;

public class BasicInteraction : MonoBehaviour, IHighlightable, IInteractable
{
    [SerializeField] UnityEvent _onInteract = new UnityEvent();
    [SerializeField] UnityEvent _onHighlightEnter = new UnityEvent();
    [SerializeField] UnityEvent _onHighlightExit = new UnityEvent();

    public bool Interactible => true;

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

    public void Interact(Player player)
    {
        _onInteract?.Invoke();
    }
}