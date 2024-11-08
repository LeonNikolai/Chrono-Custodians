using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization;

public class Item : NetworkBehaviour, IInteractable, IEquippable, IInventoryItem, IScanable
{

    [SerializeField] ItemData _itemData;
    public enum MinigameType
    {
        None,
        PointScanningWorld,
    }
    [Header("Settings")]
    [SerializeField] public bool Droppable = true;
    [SerializeField] public bool UseUniqueVisualsWhenInHand = false;
    [SerializeField] Renderer[] _handMeshRenderers = new Renderer[0];
    [Header("Item Refferences")]
    [SerializeField] public MinigameType _requiresMinigameToScan = MinigameType.PointScanningWorld;
    [SerializeField] Renderer[] _meshRenderers = new Renderer[0];
    [SerializeField] Collider[] _collider = new Collider[0];
    public Collider[] Collider => _collider;
    public ulong RandomSeed => NetworkObject.NetworkObjectId;
    public int InStabilityWorth { get; set; } = 0;
    NetworkVariable<ItemSlotType> currentSlot = new NetworkVariable<ItemSlotType>(ItemSlotType.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    internal NetworkVariable<bool> isPickedUpByPlayer = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> HasBeenScanned = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isInteractable = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public static int CalculateRemainingUnstableItemInstability(TimePeriod currentTimePeriod, bool despawnItems = false)
    {
        int instability = 0;
        foreach (var item in Item.AllItems)
        {
            bool itemBellongsInPeriod = item.ItemData.TimePeriods.Contains(currentTimePeriod);
            bool sendable = !item.ItemData.UnSendable;

            if (sendable)
            {
                if (itemBellongsInPeriod)
                {
                    instability += item.InStabilityWorth;
                }
                if (despawnItems) item.NetworkObject.Despawn(true);
            }
            // bool isPickedUpByPlayer = item.isPickedUpByPlayer.Value;
        }
        return instability;
    }
    public bool InInventory
    {
        get
        {
            return !IsOwnedByServer;
        }
    }

    public ItemData ItemData => _itemData;

    public bool Interactable => isInteractable.Value;

    public virtual string ScanTitle => _itemData ? _itemData.Name : "Unknown Item";

    public virtual string ScanResult
    {
        get
        {

            if (_itemData)
            {
                if (_requiresMinigameToScan != MinigameType.None)
                {
                    if (!HasBeenScanned.Value) return "This item needs to be scanned";
                    return ListScannedMinigameResults(_itemData.ScanMinigameResults, _itemData.ScanMinigameResults.Length);
                }
                string tags = "";
                if (_itemData.Tags.Length > 0) tags = _itemData.Tags[0].Name;
                for (int i = 1; i < _itemData.Tags.Length; i++)
                {
                    if (i == _itemData.Tags.Length - 1)
                    {
                        if (_itemData.Tags.Length > 2) tags += ", ";
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
    private string ListScannedMinigameResults(LocalizedString[] results, int scannedCount)
    {
        string scanResultText = "";
        for (int i = 0; i < scannedCount; i++)
        {
            scanResultText += "\n\n" + (results[i] != null ? !results[i].IsEmpty ? results[i].GetLocalizedString() : "?" : "?");
        }
        return scanResultText;
    }
    public void Interact(Player player)
    {
        // Do logic on server
        PickupItemServerRpc();
    }
    public static HashSet<Item> AllItems => new HashSet<Item>();

    public static int ValidItems()
    {
        int count = 0;
        foreach (var item in AllItems)
        {
            if (item != null && item.NetworkObject != null && item.NetworkObject.IsSpawned)
            {
                count++;
            }
        }
        return count;
    }
    public static void DespawnAllItems()
    {
        foreach (var item in AllItems)
        {
            item.NetworkObject.Despawn(true);
        }
    }
    public static void DespawnAllDroppableAndSendable()
    {
        foreach (var item in AllItems)
        {
            if (item.Droppable && item.ItemData.UnSendable == false)
            {
                item.NetworkObject.Despawn(true);
            }
        }
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
        AllItems.Add(this);
        currentSlot.OnValueChanged += OnSlotChanged;
        isPickedUpByPlayer.OnValueChanged += OnPickedUpChanged;
        base.OnNetworkSpawn();
    }

    public override void OnDestroy()
    {
        AllItems.Remove(this);
        base.OnDestroy();
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
    public virtual void EnableVisualsWhenHand(bool enable = true)
    {
        foreach (var collider in _handMeshRenderers)
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
    public void PickupItemServer(ulong clientId, bool preferFistSlot = false)
    {
        if (IsOwnedByServer && !isPickedUpByPlayer.Value)
        {
            if (Player.Players.TryGetValue(clientId, out Player Character))
            {
                if (Character.Inventory.TryAddItem(NetworkObject, preferFistSlot))
                {
                    currentSlot.Value = ItemSlotType.PlayerInventory;
                    NetworkObject.ChangeOwnership(clientId);
                    isPickedUpByPlayer.Value = true;
                }
            }
        }
    }
    public void Drop(Vector3? position)
    {
        if (!Droppable) return;
        if (IsOwner && !IsHost)
        {
            DropServerRpc();
        }
        else if (IsServer)
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
                currentSlot.Value = ItemSlotType.None;
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
        if (IsOwner)
        {
            if (_requiresMinigameToScan != MinigameType.None && !HasBeenScanned.Value)
            {
                Hud.ScannerNotification = "This item requires a minigame to scan";
                return;
            }
            Hud.ScannerNotification = ScanResult;
        }
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
        bool isPickedUpByPlayer = this.isPickedUpByPlayer.Value;
        bool ownerHasEquipped = Player.Players.TryGetValue(OwnerClientId, out Player player) && player.Inventory.EquippedItemLocalRefference == (IEquippable)this;
        switch (newValue)
        {
            case ItemSlotType.PlayerInventory:
                EnableColliders(false);
                UpdateVisualsWhenInHand(isPickedUpByPlayer, ownerHasEquipped);
                break;
            case ItemSlotType.Machine:
                break;
            case ItemSlotType.None:
            default:
                EnableColliders(true);
                EnableVisuals(true);
                EnableVisualsWhenHand(false);
                break;
        }
    }

    private void UpdateVisualsWhenInHand(bool isPickedUpByPlayer, bool ownerHasEquipped)
    {
        if (UseUniqueVisualsWhenInHand && isPickedUpByPlayer && ownerHasEquipped)
        {
            EnableVisuals(false);
            EnableVisualsWhenHand(isPickedUpByPlayer);
        }
        else
        {
            EnableVisualsWhenHand(false);
            EnableVisuals(isPickedUpByPlayer && ownerHasEquipped);
        }
    }

    public void OnExitInventory(object inventory, ItemSlotType slotType)
    {

    }

    [Rpc(SendTo.Server)]
    public void OnScanRpc()
    {
        HasBeenScanned.Value = true;
    }
    public void OnScan(Player player)
    {
        OnScanRpc();
    }



}

public interface ItemUseToolTip
{
    string ItemToolTip { get; }
}

