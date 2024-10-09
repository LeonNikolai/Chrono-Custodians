using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour, IInteractable, IEquippable, IInventoryItem, IScanable
{

    [SerializeField] ItemData _itemData;
    [Header("Item Refferences")]
    [SerializeField] Renderer[] _meshRenderers = new Renderer[0];
    [SerializeField] Collider[] _collider = new Collider[0];
    public Collider[] Collider => _collider;
    public ulong RandomSeed => NetworkObject.NetworkObjectId;

    NetworkVariable<ItemSlotType> currentSlot = new NetworkVariable<ItemSlotType>(ItemSlotType.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    internal NetworkVariable<bool> isPickedUpByPlayer = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public bool InInventory
    {
        get
        {
            return !IsOwnedByServer;
        }
    }

    public ItemData ItemData => _itemData;

    public bool Interactible => true;

    public virtual string ScanTitle => _itemData ? _itemData.Name : "Unknown Item";

    public virtual string ScanResult
    {
        get
        {
            if(_itemData) {
                string tags = "";
                if(_itemData.Tags.Length > 0) tags = _itemData.Tags[0].Name;
                for (int i = 1; i < _itemData.Tags.Length; i++)
                {
                    if(i == _itemData.Tags.Length - 1) {
                        if(_itemData.Tags.Length > 2) tags += ", ";
                        tags += "and ";
                    }
                    else tags += ", ";
                    tags += _itemData.Tags[i].Name;
                }
                return $"{_itemData.Description}\n\n Notable Features : {tags}";
            }
            return _itemData ? _itemData.Description : "No Description\n\n";
        }
    }

    public void Interact(Player player)
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
        isPickedUpByPlayer.OnValueChanged += OnPickedUpChanged;
        base.OnNetworkSpawn();
    }




    public virtual void EnableColliders(bool enable = true)
    {
        foreach (var collider in _collider)
        {
            collider.enabled = enable;
        }
    }
    public virtual void EnableVisuals(bool enable = true)
    {
        foreach (var collider in _meshRenderers)
        {
            collider.enabled = enable;
        }
    }
    private void OnPickedUpChanged(bool previousValue, bool newValue)
    {
        HandleReparenting(OwnerClientId);
        OnSlotChanged(currentSlot.Value, currentSlot.Value);
    }
    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        base.OnOwnershipChanged(previous, current);
        HandleReparenting(current);
        OnSlotChanged(currentSlot.Value, currentSlot.Value);
    }

    private void HandleReparenting(ulong currentOwnerId)
    {

        if (isPickedUpByPlayer.Value && Player.Players.TryGetValue(currentOwnerId, out Player Character))
        {
            transform.SetParent(Character.Inventory.Hand, true);
            Debug.Log("Item is owned by player");
            if (IsOwner)
            {
                Debug.Log("Item is owned by player and is owner");
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            return;
        }
        transform.SetParent(null, true);
    }

    [Rpc(SendTo.Server)]
    public void PickupItemServerRpc(RpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        PickupItemServer(clientId);
    }
    public void PickupItemServer(ulong clientId)
    {
        if (IsOwnedByServer && !isPickedUpByPlayer.Value)
        {
            if (Player.Players.TryGetValue(clientId, out Player Character))
            {
                if (Character.Inventory.TryAddItem(NetworkObject))
                {
                    NetworkObject.ChangeOwnership(clientId);
                    currentSlot.Value = ItemSlotType.PlayerInventory;
                    isPickedUpByPlayer.Value = true;
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
                isPickedUpByPlayer.Value = false;
                transform.position = pos;
                transform.rotation = rot;
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
                player.Inventory.DropPlacement(transform);
                currentSlot.Value = ItemSlotType.None;
                isPickedUpByPlayer.Value = false;
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


    public virtual void OnEquip(object character)
    {
        OnSlotChanged(currentSlot.Value, currentSlot.Value);
        EnableColliders(false);
        EnableVisuals(true);
    }

    public virtual void OnEquipUpdate(object character)
    {
    }


    public virtual void OnUnequip(object character)
    {
        OnSlotChanged(currentSlot.Value, currentSlot.Value);
    }

    public void OnEnterInventory(object inventory, ItemSlotType slotType)
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

    public void OnExitInventory(object inventory, ItemSlotType slotType)
    {

    }

    public void OnScan(Player player)
    {

    }
}

public interface ItemUseToolTip
{
    string ItemToolTip { get; }
}