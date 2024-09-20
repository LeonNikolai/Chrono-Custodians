using System;
using Unity.Netcode;
using UnityEngine;

public class StationaryItemScanner : NetworkBehaviour, IInteractable
{
    public Item _itemReference = null;
    public NetworkVariable<NetworkObjectReference> _itemInSystem = new NetworkVariable<NetworkObjectReference>();

    public void Interact(PlayerMovement player)
    {
        InteractServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void InteractServerRpc(RpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        if (IsOwnedByServer)
        {
            if (Player.Players.TryGetValue(clientId, out Player Character))
            {
                var item = Character.Inventory.EquippedItemLocalRefference;
                if (Character.Inventory.EquippedItemLocalRefference != null)
                {
                    Character.Inventory.EquippedItemLocalRefference.Drop(transform.position);
                    item.OnEquip(this);
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {

        _itemInSystem.OnValueChanged += OnItemChanged;
    }
    private void OnItemChanged(NetworkObjectReference previousValue, NetworkObjectReference newValue)
    {
        if (previousValue.TryGet(out var networkObject))
        {
            if (networkObject.TryGetComponent(out Item item))
            {
                item.OnUnequip(this);
            }
        }
        _itemReference = null;
        if (newValue.TryGet(out networkObject))
        {
            if (networkObject.TryGetComponent(out Item item))
            {
                item.OnEquip(this);
                _itemReference = item;
                Debug.Log(_itemReference.ItemData.Name);
            }
        }


    }
}