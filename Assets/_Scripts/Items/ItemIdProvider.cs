using UnityEngine;

[CreateAssetMenu(menuName = "Chrono Custodians/ItemIdProvider")]
public class ItemIdProvider : ScriptableObject
{
    [SerializeField] private ItemData[] _itemData;
    [SerializeField] private TimePeriod[] _timePeriod;
    public int GetId(ItemData itemData)
    {
        for (int i = 0; i < _itemData.Length; i++)
        {
            if (_itemData[i] == itemData)
            {
                return i;
            }
        }
        Debug.LogError("ItemData not found in ItemIdProvider");
        return -1;
    }
    public int GetId(TimePeriod timePeriod)
    {
        for (int i = 0; i < _timePeriod.Length; i++)
        {
            if (_timePeriod[i] == timePeriod)
            {
                return i;
            }
        }
        Debug.LogError("timePeriod not found in ItemIdProvider");
        return -1;
    }
    public ItemData GetItemData(int id)
    {
        if (id < 0 || id >= _itemData.Length)
        {
            Debug.LogError("ItemData not found in ItemIdProvider");
        }
        return _itemData[id];
    }
    public TimePeriod GetPeriodData(int id)
    {
        if (id < 0 || id >= _timePeriod.Length)
        {
            Debug.LogError($"TimePeriod with ID: {id} not found in ItemIdProvider");
        }
        return _timePeriod[id];
    }
}