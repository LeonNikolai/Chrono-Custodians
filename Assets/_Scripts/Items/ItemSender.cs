using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public struct ItemSendEvent : INetworkSerializable, System.IEquatable<ItemSendEvent>
{
    public int ItemID;
    public ItemData ItemData
    {
        get
        {
            return GameManager.IdProvider.GetItemData(ItemID);
        }
        set
        {
            ItemID = GameManager.IdProvider.GetItemId(value);
        }
    }
    public int LevelId;
    public LevelScene Level
    {
        get
        {
            return GameManager.IdProvider.GetLevelScene(LevelId);
        }
        set
        {
            LevelId = GameManager.IdProvider.GetLevelSceneId(value);
        }
    }
    public TimePeriod TargetPeriod
    {
        get
        {
            return GameManager.IdProvider.GetPeriodData(TargetPeriodID);
        }
        set
        {
            LevelId = GameManager.IdProvider.GetTimeId(value);
        }
    }
    public int InstabilityWorth;
    public int TargetPeriodID;

    public bool Equals(ItemSendEvent other)
    {
        return ItemID == other.ItemID && LevelId == other.LevelId && InstabilityWorth == other.InstabilityWorth && TargetPeriodID == other.TargetPeriodID;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref InstabilityWorth);
        serializer.SerializeValue(ref TargetPeriodID);
        serializer.SerializeValue(ref ItemID);
        serializer.SerializeValue(ref LevelId);
    }
}

public class ItemSender : NetworkBehaviour, IInteractable, IInteractionMessage, IScanable
{
    [NonSerialized] public NetworkVariable<int> SelectedPeriodID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public void SetSelectedTimePeriod(TimePeriod period)
    {
        int id = idProvider.GetTimeId(period);
        SetSelectedYearServerRpc(id);
    }
    [Rpc(SendTo.Server)]
    private void SetSelectedYearServerRpc(int id)
    {
        SelectedPeriodID.Value = id;
    }

    private void OnPeriodIDChanged(int previous, int current)
    {
        if (current == -1)
        {
            sendingItemScreen.SetActive(false);
            return;
        }
        sendingItemScreen.SetActive(true);
        sendingItemText.text = idProvider.GetPeriodData(current).periodName;
        if (IsHost) StartSending();
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
        // if (IsServer)
        // {
        //     SelectedPeriodID.Value = LevelManager.LoadedScene ? idProvider.GetTimeId(LevelManager.LoadedScene.TimePeriod) : 0;
        // }
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
                if (currentlyHeldItem)
                {
                    currentlyHeldItem.OnPickedUp += OnPickup;
                }
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

    private void OnPickup()
    {
        if (currentlyHeldItem)
        {
            currentlyHeldItem.OnPickedUp -= OnPickup;
            currentlyHeldItem = null;
            StopAllCoroutines();
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
    public override void OnDestroy()
    {
        if (currentlyHeldItem != null)
        {
            currentlyHeldItem.OnPickedUp -= OnPickup;
        }
        base.OnDestroy();
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
        if (currentlyHeldItem == null)
        {
            Debug.LogError("No item to send");
            yield break;
        }

        int Instability = currentlyHeldItem.InStabilityWorth;
        currentlyHeldItem.OnPickedUp -= OnPickup;
        if (currentlyHeldItem.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Despawn();
        }
        else
        {
            Destroy(currentlyHeldItem.gameObject);
        }

        currentlyHeldItem = null;

        if (LevelManager.LoadedScene == null)
        {
            Debug.LogError("No level loaded to send item from");
            yield break;
        }
        if (SelectedPeriodID.Value == -1)
        {
            Debug.LogError("No time period selected to send item to");
            yield break;
        }
        if (itemData == null)
        {
            Debug.LogError("No item data to send");
            yield break;
        }
        ItemSendEvent item = new ItemSendEvent
        {
            ItemData = itemData,
            Level = LevelManager.LoadedScene,
            TargetPeriodID = SelectedPeriodID.Value,
            InstabilityWorth = Instability
        };
        SentItems.Add(item);
        OnItemSendServer.Invoke(item);
    }

    public void OnScan(Player player)
    {

    }
}