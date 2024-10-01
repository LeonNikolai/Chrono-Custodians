using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public class ItemSender : NetworkBehaviour, IInteractable, IInteractionMessage
{
    public int TargetYear = 0;
    [SerializeField] private LocalizedString _interactionMessage;
    public string InteractionMessage => _interactionMessage.GetLocalizedString();
    [SerializeField] private LocalizedString _cantInteractMessage;
    public string CantInteractMessage => _cantInteractMessage.GetLocalizedString();
    public static UnityEvent<ItemData> OnItemSendServer;

    public struct ItemSendEvent {
        public ItemData ItemData;
        public LevelScene level;
        public int TargetYear;

    }
    public static List<ItemSendEvent> SentItems = new List<ItemSendEvent>();
    public static void ClearSentItems()
    {
        SentItems.Clear();
    }
    private void Awake()
    {
        if (OnItemSendServer == null) OnItemSendServer = new();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        TargetYear = LevelManager.LoadedScene ? LevelManager.LoadedScene.AstronomicalYear : 0;
    }
    public bool Interactible
    {
        get
        {
            if (Player.LocalPlayer == null) return false;
            if (Player.LocalPlayer.Inventory == null) return false;
            if (Player.LocalPlayer.Inventory.EquippedItemLocalRefference == null) return false;
            if (Player.LocalPlayer.Inventory.EquippedItemLocalRefference is Item item && item.ItemData?.UnSendable == false)
            {
                return true;
            }
            return false;
        }
    }

    public void Interact(Player player)
    {
        TrySendEquippedItemServerRpc();

    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    internal void TrySendEquippedItemServerRpc(RpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;

        if (Player.Players.TryGetValue(clientId, out Player Character))
        {
            if (Character == null) return;
            if (Character.Inventory.EquippedItemLocalRefference == null) return;
            if (Character.Inventory.EquippedItemLocalRefference is Item item && item.ItemData?.UnSendable == false)
            {
                ItemData itemData = item.ItemData;
                item.Drop(null);
                item.NetworkObject.Despawn(true);
                SendItem(itemData);
            }
        }
    }

    private void SendItem(ItemData itemData)
    {
        SentItems.Add(new ItemSendEvent {
            ItemData = itemData,
            level = LevelManager.LoadedScene,
            TargetYear = TargetYear
        });
        OnItemSendServer.Invoke(itemData);
    }
}

