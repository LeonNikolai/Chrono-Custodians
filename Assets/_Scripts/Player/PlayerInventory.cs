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

    // Inventory
    public NetworkList<NetworkObjectReference> Inventory;

    // Equipped Item
    public NetworkVariable<NetworkObjectReference> EquippedNetworkItem = new NetworkVariable<NetworkObjectReference>(new NetworkObjectReference(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public IEquippable _equippedItemLocalRefference = null;
    public IEquippable EquippedItemLocalRefference
    {
        get => _equippedItemLocalRefference;
        private set
        {
            if (_equippedItemLocalRefference == value) return;
            _equippedItemLocalRefference?.OnUnequip(this);
            _equippedItemLocalRefference = value;
            _equippedItemLocalRefference?.OnEquip(this);
        }
    }
    private void Awake()
    {
        Inventory = new NetworkList<NetworkObjectReference>(new List<NetworkObjectReference>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!_player)
        {
            _player = GetComponent<Player>();
        }

        if (IsServer)
        {
            EquippedNetworkItem.Value = new NetworkObjectReference(NetworkObject);
        }
        EquippedNetworkItem.OnValueChanged += EquippedItemChange;
        base.OnNetworkSpawn();
    }

    private void Update()
    {
        EquippedItemLocalRefference?.OnEquipUpdate(this);
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            EquippedItemLocalRefference?.Drop();
        }
    }
    public override void OnDestroy()
    {
        Inventory.Dispose();
        EquippedNetworkItem.Dispose();
        base.OnDestroy();
    }

    private void EquippedItemChange(NetworkObjectReference previousValue, NetworkObjectReference newValue)
    {

        // Un-EquippItem
        if (previousValue.TryGet(out NetworkObject oldEqippedObject, NetworkManager.Singleton))
        {
            if (oldEqippedObject.TryGetComponent<IEquippable>(out var equippable))
            {

                EquippedItemLocalRefference = equippable;
            }
        }

        bool didEquipp = false;

        // Eqipp Item
        if (newValue.TryGet(out NetworkObject newEqippedObject, NetworkManager.Singleton))
        {
            if (newEqippedObject.TryGetComponent<IEquippable>(out var equippable))
            {
                didEquipp = true;
                EquippedItemLocalRefference = equippable;
            }
        }

        if (!didEquipp) EquippedItemLocalRefference = null;
    }

    public void DropPlacement(Transform dropItem)
    {
        const float MaxReachForward = 2f;
        const float MaxDownwardsDrop = 100f;
        var ray = new Ray(_player.HeadTransform.position, _player.HeadTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, MaxReachForward, _dropIgnoreLayers))
        {
            if (Physics.Raycast(hit.point, Vector3.down, out RaycastHit groundHit, MaxDownwardsDrop))
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