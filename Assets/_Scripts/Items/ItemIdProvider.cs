using UnityEngine;

[CreateAssetMenu(menuName = "Chrono Custodians/ItemIdProvider")]
public class ItemIdProvider : ScriptableObject
{
    [SerializeField] private ItemData[] _itemData;
    [SerializeField] private ItemData[] time;
    
     
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
    public ItemData GetItemData(int id)
    {
        if (id < 0 || id >= _itemData.Length)
        {
            Debug.LogError("ItemData not found in ItemIdProvider");
        }
        return _itemData[id];
    }
}