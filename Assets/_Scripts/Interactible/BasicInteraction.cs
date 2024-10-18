using UnityEngine;
using UnityEngine.Events;

public class BasicInteraction : MonoBehaviour, IHighlightable, IInteractable
{
    [SerializeField] UnityEvent _onInteract = new UnityEvent();
    [SerializeField] UnityEvent _onHighlightEnter = new UnityEvent();
    [SerializeField] UnityEvent _onHighlightExit = new UnityEvent();
    public UnityEvent OnInteract => _onInteract;
    public UnityEvent OnHighlightEnter => _onHighlightEnter;
    public UnityEvent OnHighlightExit => _onHighlightExit;

    public bool Interactable => true;

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