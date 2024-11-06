using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public struct ItemSendEvent
{
    public ItemData ItemData;
    public LevelScene level;
    public int TargetPeriodID;

}

public class ItemSender : NetworkBehaviour, IInteractable, IInteractionMessage, IScanable
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
        StartSending();
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
    public bool Interactable
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
    private Item currentlyHeldItem;
    private NetworkVariable<bool> sendingItems = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
            if (Character.Inventory.EquippedItemLocalRefference is Item item && item.ItemData?.UnSendable == false && currentlyHeldItem == null)
            {
                ItemData itemData = item.ItemData;
                item.Drop(itemDropSpot.position);
                currentlyHeldItem = item;
                if (sendingItems.Value)
                {
                    StartCoroutine(SendItem(itemData));
                }
            }
        }

        if (curItemSendCooldown <= 0)
        {


        }
    }

    public void StartSending()
    {
        if (currentlyHeldItem != null && !sendingItems.Value && currentlyHeldItem.transform.position != itemDropSpot.position)
        {
            currentlyHeldItem = null;
        }
        if (currentlyHeldItem == null)
        {
            hatchAnim.SetBool("Open", true);
            sendingItems.Value = true;
        }
        else
        {
            StartCoroutine(SendItem(currentlyHeldItem.ItemData));
        }
    }

    public void StopSending()
    {
        if (sendingItems.Value)
        {
            StopSendingRPC();
            hatchAnim.SetBool("Open", false);
        }
    }

    [Rpc(SendTo.Server)]
    private void StopSendingRPC()
    {
        sendingItems.Value = false;
    }



    [SerializeField] private Animator hatchAnim;
    private IEnumerator SendItem(ItemData itemData)
    {
        currentlyHeldItem.isInteractable.Value = false;

        float timer = 0;
        Vector3 oldPos = currentlyHeldItem.transform.position;
        Vector3 newPos = currentlyHeldItem.transform.position + Vector3.down;

        if (!sendingItems.Value)
        {
            hatchAnim.SetBool("Open", true);
            yield return new WaitForSeconds(0.5f);
            sendingItems.Value = true;
        }

        while (timer < 1)
        {
            yield return null;
            timer += Time.deltaTime * 2;
            currentlyHeldItem.transform.position = Vector3.Lerp(oldPos, newPos, timer);
        }

        currentlyHeldItem.GetComponent<NetworkObject>().Despawn();
        currentlyHeldItem = null;

        ItemSendEvent item = new ItemSendEvent
        {
            ItemData = itemData,
            level = LevelManager.LoadedScene,
            TargetPeriodID = SelectedPeriodID.Value
        };
        SentItems.Add(item);
        OnItemSendServer.Invoke(item);
    }

    public void OnScan(Player player)
    {

    }
}