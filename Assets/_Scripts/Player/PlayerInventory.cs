using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : NetworkBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] Transform _playerHand;
    [SerializeField] LayerMask _dropIgnoreLayers = ~0;
    public Transform Hand => _playerHand ? _playerHand : _player.transform;
    public Transform Head => _player.HeadTransform;
    public Player Player => _player;

    const int InventorySize = 6;
    // Inventory
    public NetworkList<NetworkObjectReference> Inventory;

    public NetworkObjectReference CurrentEquippedNetworkObject => Inventory[EquippedNetworkItemIndex.Value];
    public bool TryAddItem(NetworkObject item, bool preferFistSlot = false)
    {
        if (item == null) return false;
        // Prevent adding to slot that is out of bounds
        if (EquippedNetworkItemIndex.Value >= InventorySize)
        {
            return false;
        }
        // Prevent replacing item
        if (preferFistSlot || Inventory[EquippedNetworkItemIndex.Value].TryGet(out NetworkObject currentItem))
        {
            // Try find empty slot
            for (int i = 0; i < Inventory.Count; i++)
            {
                if (!Inventory[i].TryGet(out NetworkObject current))
                {
                    Inventory[i] = new NetworkObjectReference(item);
                    return true;
                }
            }
            return false;
        }
        Inventory[EquippedNetworkItemIndex.Value] = new NetworkObjectReference(item);
        return true;
    }

    public bool TryRemoveItem(NetworkObject item)
    {
        if (item == null) return false;
        for (int i = 0; i < Inventory.Count; i++)
        {
            if (Inventory[i].TryGet(out NetworkObject currentItem))
            {
                if (currentItem == item)
                {
                    Debug.Log("Removing item: " + item);
                    Inventory[i] = new NetworkObjectReference();
                    return true;
                }
            }
        }
        Debug.LogWarning("Item not found in inventory: " + item);
        return false;
    }

    // Equipped Item
    [NonSerialized] public NetworkVariable<byte> EquippedNetworkItemIndex = new NetworkVariable<byte>(2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public IEquippable _equippedItemLocalRefference = null;
    public IEquippable EquippedItemLocalRefference
    {
        get => _equippedItemLocalRefference;
        private set
        {
            if (_equippedItemLocalRefference == value) return;
            var oldItem = _equippedItemLocalRefference;
            _equippedItemLocalRefference = value;
            if (oldItem != null)
            {
                Debug.Log("Unequipping: " + oldItem);
                oldItem.OnUnequip(_player);
            }
            if (_equippedItemLocalRefference != null)
            {
                Debug.Log("Equipping: " + _equippedItemLocalRefference);
                _equippedItemLocalRefference.OnEquip(_player);
            }
            if (IsOwner)
            {
                if (EquippedItemLocalRefference is ItemUseToolTip toolTip)
                {
                    Hud.ItemTooltip = toolTip?.ItemToolTip;
                }
                else
                {
                    Hud.ItemTooltip = "";


                }
                if (EquippedItemLocalRefference == null) Hud.ScannerNotification = "";
            }
        }
    }

    private void Awake()
    {
        var list = new List<NetworkObjectReference>() { };
        for (int i = 0; i < InventorySize; i++)
        {
            list.Add(new NetworkObjectReference());
        }
        Inventory = new NetworkList<NetworkObjectReference>(list, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!_player)
        {
            _player = GetComponent<Player>();
        }

        EquippedNetworkItemIndex.OnValueChanged += EquippedItemChange;
        Inventory.OnListChanged += InventoryChanged;
        if (IsOwner)
        {
            EquippedNetworkItemIndex.Value = 0;
        }
        if (IsServer)
        {
            Player.Health.onDeath.AddListener(dead =>
            {
                if (dead)
                {
                    for (int i = 0; i < Inventory.Count; i++)
                    {
                        if (Inventory[i].TryGet(out NetworkObject currentItem))
                        {
                            if (currentItem.TryGetComponent<Item>(out var inventoryItem))
                            {
                                if (inventoryItem.Droppable)
                                {
                                    inventoryItem.Drop(null);
                                    Inventory[i] = new NetworkObjectReference();
                                }
                            }
                        }
                    }
                }
            });
        }
        base.OnNetworkSpawn();
    }
    public override void OnDestroy()
    {
        // Inventory?.Dispose();
        base.OnDestroy();
    }


    private void InventoryChanged(NetworkListEvent<NetworkObjectReference> changeEvent)
    {

        switch (changeEvent.Type)
        {
            case NetworkListEvent<NetworkObjectReference>.EventType.Add:
                if (changeEvent.Value.TryGet(out NetworkObject newItem))
                {
                    if (newItem.TryGetComponent<IInventoryItem>(out var inventoryItem))
                    {
                        inventoryItem.OnEnterInventory(this, ItemSlotType.PlayerInventory);
                        if (changeEvent.Index == EquippedNetworkItemIndex.Value)
                        {
                            EquippedItemLocalRefference = inventoryItem as IEquippable;
                        }
                    }
                }
                break;
            case NetworkListEvent<NetworkObjectReference>.EventType.Remove:
                if (changeEvent.Value.TryGet(out NetworkObject oldItem))
                {
                    if (oldItem.TryGetComponent<IInventoryItem>(out var inventoryItem))
                    {
                        inventoryItem.OnExitInventory(this, ItemSlotType.PlayerInventory);
                        if (inventoryItem is IEquippable equippable && EquippedItemLocalRefference == equippable)
                        {
                            EquippedItemLocalRefference = null;
                        }
                    }
                }
                break;
            case NetworkListEvent<NetworkObjectReference>.EventType.Value:
                {
                    if (changeEvent.Value.TryGet(out NetworkObject newItemValue))
                    {
                        if (newItemValue.TryGetComponent<IInventoryItem>(out var inventoryItemEnter))
                        {
                            inventoryItemEnter.OnEnterInventory(this, ItemSlotType.PlayerInventory);
                        }
                        if (changeEvent.Index == EquippedNetworkItemIndex.Value)
                        {
                            if (newItemValue.TryGetComponent<IEquippable>(out var equippableItem))
                            {
                                EquippedItemLocalRefference = equippableItem;
                            }
                            else
                            {
                                EquippedItemLocalRefference = null;
                            }
                        }
                    }
                    else
                    {
                        if (changeEvent.Index == EquippedNetworkItemIndex.Value)
                        {
                            EquippedItemLocalRefference = null;
                        }
                    }

                    if (changeEvent.PreviousValue.TryGet(out NetworkObject removedItem))
                    {
                        if (removedItem.TryGetComponent<IInventoryItem>(out var inventoryItemExit))
                        {
                            inventoryItemExit.OnExitInventory(this, ItemSlotType.PlayerInventory);
                        }
                    }
                }
                break;
            default:
                Debug.LogWarning("Unhandled inventory change event: " + changeEvent.Type);
                break;
        }

        UpdateHud();

    }

    private void EquippedItemChange(byte previousValue, byte newValue)
    {
        UpdateHud();
        if (Inventory.Count == 0)
        {
            EquippedItemLocalRefference = null;
            return;
        }
        if (newValue >= Inventory.Count)
        {
            EquippedItemLocalRefference = null;
            return;
        }
        var newObject = Inventory[newValue];
        if (newObject.TryGet(out NetworkObject newEqippedObject))
        {
            if (newEqippedObject.TryGetComponent<IEquippable>(out var equippable))
            {
                EquippedItemLocalRefference = equippable;
                return;
            }
        }
        EquippedItemLocalRefference = null;
    }

    private void UpdateHud()
    {
        if (!IsOwner) return;

        for (int i = 0; i < InventorySize; i++)
        {
            if (Inventory[i].TryGet(out NetworkObject item))
            {
                if (item.TryGetComponent<IInventoryItem>(out var inventoryItem))
                {
                    Hud.SetInventoryIcon(inventoryItem.ItemData, i, i == EquippedNetworkItemIndex.Value);
                }
                else
                {
                    Hud.SetInventoryIcon(null, i, i == EquippedNetworkItemIndex.Value);
                }
            }
            else
            {
                Hud.SetInventoryIcon(null, i, i == EquippedNetworkItemIndex.Value);
            }
        }
    }

    private void Update()
    {
        EquippedItemLocalRefference?.OnEquipUpdate(this);
        if (!IsOwner) return;
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            EquippedItemLocalRefference?.Drop();
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            EquippedNetworkItemIndex.Value = 0;
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            EquippedNetworkItemIndex.Value = 1;
        }
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            EquippedNetworkItemIndex.Value = 2;
        }
        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            EquippedNetworkItemIndex.Value = 3;
        }
        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            EquippedNetworkItemIndex.Value = 4;
        }
        if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            EquippedNetworkItemIndex.Value = 5;
        }
        if (Keyboard.current.digit7Key.wasPressedThisFrame)
        {
            EquippedNetworkItemIndex.Value = 6;
        }
        if (Keyboard.current.digit8Key.wasPressedThisFrame)
        {
            EquippedNetworkItemIndex.Value = 7;
        }
        if (Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            EquippedNetworkItemIndex.Value = 8;
        }
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            EquippedNetworkItemIndex.Value = 9;
        }
    }




    public void DropPlacement(Transform dropItem)
    {
        const float MaxReachForward = 2f;
        const float MaxDownwardsDrop = 100f;
        var ray = new Ray(_player.HeadTransform.position, _player.HeadTransform.forward);

        // Raycast until we hit something, then raycast downwards from that point to find the ground

        if (Physics.Raycast(ray, out RaycastHit hit, MaxReachForward, _dropIgnoreLayers))
        {
            var hitnormal = hit.normal;
            bool isGrounded = Vector3.Dot(hitnormal, Vector3.up) > 0.5f;
            if (isGrounded)
            {
                dropItem.position = hit.point;
                dropItem.rotation = Quaternion.LookRotation(Vector3.forward, hitnormal);
                return;
            }
            if (Physics.Raycast(Vector3.MoveTowards(hit.point, _player.transform.position, 0.1f), Vector3.down, out RaycastHit groundHit, MaxDownwardsDrop))
            {
                dropItem.position = groundHit.point;
                var normal = groundHit.normal;
                dropItem.rotation = Quaternion.LookRotation(Vector3.forward, normal);
            }
            else
            {
                dropItem.position = transform?.position ?? hit.point;
                dropItem.rotation = Quaternion.identity;
            }
        }
        else
        {
            var airPos = transform.position = ray.GetPoint(2f);
            if (Physics.Raycast(airPos, Vector3.down, out RaycastHit airHit, MaxDownwardsDrop, _dropIgnoreLayers))
            {
                dropItem.position = airHit.point;
                var normal = airHit.normal;
                dropItem.rotation = Quaternion.LookRotation(Vector3.forward, normal);
            }
            else
            {
                dropItem.position = transform?.position ?? airPos;
            }
        }
    }

}