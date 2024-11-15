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

public interface ILongInteractable
{
    void LongInteract(Player player);
    public float InteractTime { get; }
}
interface IInteractionMessage
{
    string InteractionMessage { get; }
    string CantInteractMessage { get; }
}