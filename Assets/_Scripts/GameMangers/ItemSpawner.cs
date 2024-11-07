using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    public LevelScene level;
    public ItemIdProvider idProvider;
    public ItemData[] _normalItems;
    public ItemData[] _forignItems;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (_normalItems.Length == 0)
            {
                Debug.LogError("No normal items assigned to ItemManager");
                return;
            }
            if (_forignItems.Length == 0)
            {
                Debug.LogError("No forign items assigned to ItemManager");
                return;
            }
        }
    }

    public static ItemData[] Shuffle(ItemData[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            ItemData temp = items[i];
            int randomIndex = Random.Range(i, items.Length);
            items[i] = items[randomIndex];
            items[randomIndex] = temp;
        }
        return items;
    }

    private void SpawnItems()
    {
        float unstableItemCount = GameManager.LevelUnstableItemSpawnCount;

        var spawnpoints = ItemSpawnPoint.GetShuffledSpawnPoints();
        var currentTimePeriod = GameManager.instance.GetCurrentLevelStability().scene.TimePeriod;

        // remove items that dont fit its designated time period
        var unstableItems = Shuffle(_forignItems).TakeWhile(item => item.TimePeriods.Contains(currentTimePeriod)).ToArray();
        var normalItems = Shuffle(_normalItems).TakeWhile(item => !item.TimePeriods.Contains(currentTimePeriod)).ToArray();

        int maxItems = spawnpoints.Count;
        int i = 0;
        while (unstableItemCount > 0)
        {
            var item = unstableItems[i % unstableItems.Length];
            var spawnpoint = spawnpoints[i % spawnpoints.Count];
            SpawnItem(item, spawnpoint);
            unstableItemCount--;
            i++;
        }

        while (i < maxItems)
        {
            var item = normalItems[i % normalItems.Length];
            var spawnpoint = spawnpoints[i % spawnpoints.Count];
            SpawnItem(item, spawnpoint);
            i++;
        }
    }

    [ContextMenu("Autofill")]
    private void Autofill()
    {
        if(idProvider == null)
        {
            Debug.LogError("No ItemIdProvider assigned to ItemManager");
        }
        if(level == null)
        {
            Debug.LogError("No TimePeriod assigned to ItemManager");
        }
        var periods = level.TimePeriod;
        _normalItems = idProvider.ItemDatas.Where(item => !item.TimePeriods.Contains(periods)).ToArray();
        _forignItems = idProvider.ItemDatas.Where(item => item.TimePeriods.Contains(periods)).ToArray();
    }
    private ItemData GetItem()
    {
        int randomIndex = UnityEngine.Random.Range(0, _forignItems.Length);
        ItemData item = _forignItems[randomIndex];
        return item;
    }

    private void SpawnItem(ItemData itemData, Transform spawnpoint)
    {
        var item = Instantiate(itemData.Prefab, spawnpoint.position, spawnpoint.transform.rotation);
        if (item.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Spawn(true);
        }
        else
        {
            Debug.LogError("Item prefab does not have a NetworkObject component");
        }
    }
}