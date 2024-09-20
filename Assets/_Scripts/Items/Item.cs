using System;
using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour, IInteractable, IEquippable, IInventoryItem, IHighlightable
{

    bool hasItemPhysics = false;
    [SerializeField] ItemData _itemData;
    private Transform _transformTarget = null;

    [Header("Item Refferences")]
    [SerializeField] Renderer[] _meshRenderers = new Renderer[0];
    [SerializeField] Collider[] _collider = new Collider[0];
    public Collider[] Collider => _collider;

    NetworkVariable<ItemSlotType> currentSlot = new NetworkVariable<ItemSlotType>(ItemSlotType.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool InInventory
    {
        get
        {
            return !IsOwnedByServer;
        }
    }

    public ItemData ItemData => _itemData;

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
    }
    public override void OnNetworkSpawn()
    {
        currentSlot.OnValueChanged += OnSlotChanged;
        base.OnNetworkSpawn();
    }


    public override void OnGainedOwnership()
    {
        _transformTarget = Player.LocalPlayer.Inventory.Hand;
        OnSlotChanged(currentSlot.Value, currentSlot.Value);
        base.OnGainedOwnership();
    }

    public override void OnLostOwnership()
    {
        _transformTarget = null;
        OnSlotChanged(currentSlot.Value, currentSlot.Value);
        base.OnLostOwnership();
    }

    public void EnableColliders(bool enable = true)
    {
        foreach (var collider in _collider)
        {
            collider.enabled = enable;
        }
    }
    public void EnableVisuals(bool enable = true)
    {
        foreach (var collider in _meshRenderers)
        {
            collider.enabled = enable;
        }
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
                if (Character.Inventory.TryAddItem(NetworkObject))
                {
                    NetworkObject.ChangeOwnership(clientId);
                    currentSlot.Value = ItemSlotType.PlayerInventory;
                    if (NetworkObject.TrySetParent(Character.transform, false))
                    {
                        transform.localEulerAngles = Vector3.zero;
                    }
                    else
                    {
                        Debug.Log("Didnt reparrent");
                    }
                }
            }
        }
    }
    public void Drop(Vector3? position)
    {
        if (IsOwner)
        {
            DropServerRpc();
        }
        else if (IsServer && !IsOwnedByServer)
        {
            if (position.HasValue)
            {
                DropFromOwner(position.Value, Quaternion.identity);
            }
            else
            {
                DropFromOwner();
            }
        }
    }

    private void DropFromOwner(Vector3 pos, Quaternion rot)
    {
        if (Player.Players.TryGetValue(OwnerClientId, out Player player))
        {
            if (player.Inventory.TryRemoveItem(NetworkObject))
            {
                NetworkObject.RemoveOwnership();
                if (NetworkObject.TryRemoveParent(true))
                {
                    currentSlot.Value = ItemSlotType.None;
                    transform.position = pos;
                    transform.rotation = rot;
                }
                else
                {
                    Debug.Log("Didnt reparrent");
                }
            }
        }

    }
    private void DropFromOwner()
    {

        if (Player.Players.TryGetValue(OwnerClientId, out Player player))
        {
            if (player.Inventory.TryRemoveItem(NetworkObject))
            {
                NetworkObject.RemoveOwnership();
                if (NetworkObject.TryRemoveParent(true))
                {
                    currentSlot.Value = ItemSlotType.None;
                    player.Inventory.DropPlacement(transform);
                }
                else
                {
                    Debug.Log("Didnt reparrent");
                }
            }
        }

    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    public void DropServerRpc(RpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        if (clientId == OwnerClientId)
        {
            DropFromOwner();
        }
    }


    public virtual void OnEquip(object playerCharacter)
    {
        OnSlotChanged(currentSlot.Value, currentSlot.Value);
        EnableColliders(false);
        EnableVisuals(true);
    }

    public virtual void OnEquipUpdate(object playerCharacter)
    {
        MoveHandLocation();
    }

    private void MoveHandLocation()
    {
        // Network object can only be parented to another network object, so we need to manually move the object to the hand location
        bool isPlacedInMachine = IsServer && currentSlot.Value == ItemSlotType.Machine;
        bool isHeldByPlayer = IsOwner;
        if ((isHeldByPlayer || isPlacedInMachine) && _transformTarget != null)
        {
            transform.position = _transformTarget.position;
            transform.rotation = _transformTarget.rotation;
        }
    }

    public virtual void OnUnequip(object playerCharacter)
    {
        OnSlotChanged(currentSlot.Value, currentSlot.Value);
    }

    public void OnEnterInventory(object playerCharacter, ItemSlotType slotType)
    {
        if (!IsOwner) return;
        currentSlot.Value = slotType;
    }
    private void OnSlotChanged(ItemSlotType previousValue, ItemSlotType newValue)
    {
        switch (newValue)
        {
            case ItemSlotType.PlayerInventory:
                EnableColliders(false);
                EnableVisuals(false);
                break;
            case ItemSlotType.Machine:
                break;
            case ItemSlotType.None:
            default:
                EnableColliders(true);
                EnableVisuals(true);
                break;
        }
    }

    public void OnExitInventory(object playerCharacter, ItemSlotType slotType)
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
