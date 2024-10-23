using Unity.Netcode;
using UnityEngine;

public class HealPoint : NetworkBehaviour, IInteractable, IInteractionMessage
{
    public bool Interactable => true;

    public string InteractionMessage => "Press E to heal to full";

    public string CantInteractMessage => "Heal Point is recharging";

    public void Interact(Player player)
    {
        HealToFullServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void HealToFullServerRpc(RpcParams rpcParams = default)
    {
        var id = rpcParams.Receive.SenderClientId;
        if (Player.Players.TryGetValue(id, out Player player))
        {
            player.Health.FullHeal();
        }
    }
}
