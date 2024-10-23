using Unity.Netcode;
using UnityEngine;

public class HealPoint : NetworkBehaviour, IInteractable, IInteractionMessage
{
    public bool Interactable => true;

    public string InteractionMessage => "Press E to heal to full";

    public string CantInteractMessage => "Heal Point is recharging";

    public void Interact(Player player)
    {
        HealToFullServerRPC(player.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HealToFullServerRPC(ulong playerObjectID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerObjectID, out var player))
        {
            player.GetComponent<HealthSystem>().FullHeal();
        }
    }
}
