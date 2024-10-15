using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public struct ItemSendEvent
{
    public ItemData ItemData;
    public LevelScene level;
    public TimePeriod TargetPeriod;

}

public class ItemSender : NetworkBehaviour, IInteractable, IInteractionMessage,IScanable
{
    public NetworkVariable<int> SelectedPeriodID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public void SetSelectedTimePeriod(TimePeriod period)
    {
        int id = idProvider.GetId(period);
        SetSelectedYearServerRpc(id);
    }
    [Rpc(SendTo.Server)]
    private void SetSelectedYearServerRpc(int id)
    {
        SelectedPeriodID.Value = id;
        Debug.Log("New value = " + id);
    }

    private void OnPeriodIDChanged(int previous, int current)
    {
        if (current == 0)
        {
            sendingItemScreen.SetActive(false);
            return;
        }
        sendingItemScreen.SetActive(true);
        sendingItemText.text = idProvider.GetPeriodData(current).periodName;

    }

    [SerializeField] private GameObject sendingItemScreen;
    [SerializeField] private TMP_Text sendingItemText;

    [SerializeField] private ItemIdProvider idProvider;
    [SerializeField] private LocalizedString _interactionMessage;
    public string InteractionMessage => _interactionMessage.GetLocalizedString();
    [SerializeField] private LocalizedString _cantInteractMessage;
    public string CantInteractMessage => _cantInteractMessage.GetLocalizedString();
    public static UnityEvent<ItemSendEvent> OnItemSendServer;


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
        SelectedPeriodID.OnValueChanged += OnPeriodIDChanged;
        if (IsServer)
        {
            SelectedPeriodID.Value = LevelManager.LoadedScene ? idProvider.GetId(LevelManager.LoadedScene.TimePeriod) : 0;
        }
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

    public string ScanTitle => "Temporal Mailbox";
    public string ScanResult => "Send items to the past or future";

    [SerializeField] private Transform itemDropSpot;
    private float itemSendCooldown;
    private float curItemSendCooldown;
    private ItemData currentlyHeldItem;


    public void Interact(Player player)
    {
        TrySendEquippedItemServerRpc();
    }

    [Rpc(SendTo.Server)]
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
                item.Drop(itemDropSpot.position);
                currentlyHeldItem = itemData;
            }
        }

        if (curItemSendCooldown <= 0)
        {


        }
    }

    public void PerformSendItem()
    {
        SendItem(currentlyHeldItem);
    }

    private void SendItem(ItemData itemData)
    {
        ItemSendEvent item = new ItemSendEvent 
        {
            ItemData = itemData,
            level = LevelManager.LoadedScene,
            TargetPeriod = idProvider.GetPeriodData(SelectedPeriodID.Value)
        };
        SentItems.Add(item);
        OnItemSendServer.Invoke(item);
    }

    public void OnScan(Player player)
    {

    }
}