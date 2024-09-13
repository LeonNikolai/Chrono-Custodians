using System;
using Unity.Netcode;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class Item : NetworkBehaviour, IInteractable, IEquippable, IInventoryItem, IHighlightable
{
    bool hasItemPhysics = false;
    [SerializeField] ItemData _itemData;
    private Transform _transformTarget = null;
    [SerializeField] Collider _collider;
    public Collider Collider => _collider;

    // Optional Rigidbody for dropping
    Rigidbody _rigidbody;

    public void Interact(PlayerMovement player)
    {
        // Do logic on server
        PickupItemServerRpc();
    }

    private void Awake()
    {
        if (TryGetComponent<ClientNetworkTransform>(out var clientTransform))
        {
            clientTransform.InLocalSpace = true;
        }
        if (_rigidbody == null && TryGetComponent(out Rigidbody rigidbody))
        {
            _rigidbody = rigidbody;
        }
    }
    public override void OnGainedOwnership()
    {
        if(_rigidbody) _rigidbody.isKinematic = true;
        Collider.enabled = false;
        _transformTarget = Player.LocalPlayer.Inventory.Hand;
        base.OnGainedOwnership();
    }
    public override void OnLostOwnership()
    {
        if(_rigidbody) _rigidbody.isKinematic = !hasItemPhysics;
        Collider.enabled = true;
        _transformTarget = null;
        base.OnLostOwnership();
    }

    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        base.OnOwnershipChanged(previous, current);
    }

    [Rpc(SendTo.Server)]
    public void PickupItemServerRpc(RpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        Debug.Log("Client picked up item");
        if (IsOwnedByServer)
        {
            Debug.Log("Server picked up item");
            if (Player.Players.TryGetValue(clientId, out Player Character))
            {
                NetworkObject.ChangeOwnership(clientId);
                if (NetworkObject.TrySetParent(Character.transform, false))
                {
                    transform.localEulerAngles = Vector3.zero;
                    _collider.enabled = false;
                    _transformTarget = Character.Inventory.Hand;
                }
                else
                {
                    Debug.Log("Didnt reparrent");
                }
                Character.Inventory.EquippedNetworkItem.Value = new NetworkObjectReference(NetworkObject);
            }
        }
    }
    public void Drop()
    {
        if(IsOwner) DropServerRpc();
    }
    [Rpc(SendTo.Server, RequireOwnership = true)]
    public void DropServerRpc(RpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        if (clientId == OwnerClientId)
        {
            if (Player.Players.TryGetValue(clientId, out Player player))
            {
                NetworkObject.RemoveOwnership();
                if (NetworkObject.TryRemoveParent(true))
                {
                    player.Inventory.DropPlacement(transform);
                    if(_rigidbody) _rigidbody.isKinematic = !hasItemPhysics;
                    _collider.enabled = true;
                    _transformTarget = null;
                }
                else
                {
                    Debug.Log("Didnt reparrent");
                }
                player.Inventory.EquippedNetworkItem.Value = new NetworkObjectReference();
            }
        }
    }


    public virtual void OnEquip(object playerCharacter)
    {

    }

    public virtual void OnEquipUpdate(object playerCharacter)
    {
        MoveHandLocation();
    }

    private void MoveHandLocation()
    {
        // Network object can only be parented to another network object, so we need to manually move the object to the hand location
        if (IsOwner && _transformTarget != null)
        {
            transform.position = _transformTarget.position;
            transform.rotation = _transformTarget.rotation;
        }
    }

    public virtual void OnUnequip(object playerCharacter)
    {

    }

    public void OnEnterInventory(object playerCharacter)
    {

    }

    public void OnExitInventory(object playerCharacter)
    {

    }

    public void HightlightEnter()
    {

    }

    public void HightlightUpdate()
    {

    }

    public void HightlightExit()
    {

    }


}
