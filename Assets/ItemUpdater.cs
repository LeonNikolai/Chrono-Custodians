using TMPro;
using UnityEngine;

public class ItemUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text[] textToUpdate;

    int foreignItemsleft = 0;

    private void Start()
    {
        GameManager.instance.OnLevelEndClient.AddListener(SetText);
        GameManager.instance.OnLevelStartClient.AddListener(SetSpawner);
        ItemSender.OnItemSendServer.AddListener(UpdateText);
    }

    private void SetText()
    {
        foreach(var item in textToUpdate)
        {
            item.text = "Please select a Time Period to visit";
        }
    }

    private void SetSpawner()
    {
        ItemSpawner.OnItemsSpawned.AddListener(CountItems);
    }

    private void CountItems(int unstableItems, int totalItems)
    {
        foreignItemsleft = unstableItems;
        UpdateAllText();
    }

    private void UpdateText(ItemSendEvent item)
    {
        OnItemSent(item);
        if (item.TargetPeriod != LevelManager.LoadedScene.TimePeriod)
        {
            foreignItemsleft--;
            UpdateAllText();
        }
    }

    private void UpdateAllText()
    {
        foreach(var item in textToUpdate)
        {
            item.text = $"{foreignItemsleft} foreign items remaining in this time period";
        }
    }

    private void OnItemSent(ItemSendEvent item)
    {
        bool isCorrectTimePeriod = false;
        foreach (TimePeriod period in item.ItemData.TimePeriods)
        {
            if (GameManager.instance.idProvider.GetPeriodData(item.TargetPeriodID).periodName == period.periodName)
            {
                isCorrectTimePeriod = true;
                break;
            }
        }

        if (item.ItemData.TimePeriods[0] == LevelManager.LoadedScene.TimePeriod)
        {
            return;
        }

        if (isCorrectTimePeriod)
        {
            SendClientFeedback(1, item.ItemData.NetworkedRefference, item.TargetPeriodID);
        }
        else
        {
            SendClientFeedback(0, item.ItemData.NetworkedRefference, item.TargetPeriodID);
        }
    }
    
    // Type - 0 = incorrect | 1 = correct | 2 = item should not have been sent
    private void SendClientFeedback(int type, ItemDataRefference refferencedata, int periodID)
    {
        Debug.Log("Item Tracker Received");
        ItemData data = refferencedata.Refference;
        Hud.ItemSentFeedback(type, data, GameManager.instance.idProvider.GetPeriodData(periodID));
    }
}
