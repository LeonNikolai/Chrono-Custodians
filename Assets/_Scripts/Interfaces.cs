public interface IHighlightable
{
    void HightlightEnter();

    void HightlightUpdate();

    void HightlightExit();
}

public interface IInteractable
{
    void Interact(Player player);
    public bool Interactable { get; }
}
interface IInteractionMessage
{
    string InteractionMessage { get; }
    string CantInteractMessage { get; }
}