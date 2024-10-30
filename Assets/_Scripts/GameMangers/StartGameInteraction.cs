using UnityEngine;

public class StartGameInteraction : MonoBehaviour, IInteractable
{
    public bool Interactable => true;

    public string CantInteractMessage => "Can't start game";

    public void Interact(Player player)
    {
    }
}
