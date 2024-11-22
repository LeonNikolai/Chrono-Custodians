using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ItemSpawner : NetworkBehaviour
{
    public LevelScene level;
    public ItemIdProvider idProvider;
    public ItemData[] _normalItems;
    public ItemData[] _forignItems;

    public NetworkVariable<int> foreignItemNumber = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // First int is total unstable items, second is total items
    public static UnityEvent<int, int> OnItemsSpawned = new UnityEvent<int, int>();
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            LGManager.OnLevelGenerated += SpawnItems;
            GameManager.DestroyCurrentLevel += DestroyItems;
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
    public static HashSet<NetworkObject> SpawnedNetworkObjects = new HashSet<NetworkObject>();
    private void DestroyItems()
    {
        OnItemsSpawned.RemoveAllListeners();
        try
        {
            foreach (var obj in SpawnedNetworkObjects)
            {
                obj?.Despawn(true);
            }
        }
        catch
        {

        }
        SpawnedNetworkObjects.Clear();
    }

    public override void OnDestroy()
    {
        LGManager.OnLevelGenerated -= SpawnItems;
        GameManager.DestroyCurrentLevel -= DestroyItems;
        base.OnDestroy();
    }

    public static ItemData[] Shuffle(ItemData[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            ItemData temp = items[i];
            int randomIndex = UnityEngine.Random.Range(i, items.Length);
            items[i] = items[randomIndex];
            items[randomIndex] = temp;
        }
        return items;
    }

    const int DefaultStabilityPerItem = 5;
    private void SpawnItems()
    {
        if (!IsServer) return;
        Debug.Log("! Spawning items");
        float stabilityTarget = GameManager.ActiveLevelStability;

        var spawnpoints = ItemSpawnPoint.GetShuffledSpawnPoints();
        var currentTimePeriod = GameManager.instance.GetCurrentLevelStability().scene.TimePeriod;

        // remove items that dont fit its designated time period
        var unstableItems = Shuffle(_forignItems).TakeWhile(item => item.TimePeriods.Contains(currentTimePeriod)).ToArray();
        var normalItems = Shuffle(_normalItems).TakeWhile(item => !item.TimePeriods.Contains(currentTimePeriod)).ToArray();

        int maxItems = spawnpoints.Count;
        int i = 0;

        while (stabilityTarget > 0)
        {
            var item = unstableItems[i % unstableItems.Length];
            var spawnpoint = spawnpoints[i % spawnpoints.Count];

            var itemInstability = DefaultStabilityPerItem;

            // Spawn the last item with the remaining stability
            if (maxItems == 2)
            {
                itemInstability = (int)stabilityTarget;
            }
            if (maxItems == 1) break;

            // Spawn the last item with the remaining stability
            if (stabilityTarget < DefaultStabilityPerItem)
            {
                itemInstability = (int)stabilityTarget;
            }

            SpawnItem(item, spawnpoint, itemInstability);
            stabilityTarget -= itemInstability;
            i++;
            maxItems--;
        }
        int foreignItemSpawnNumber = i;
        Debug.Log($"Spawned {i} unstable items");
        // Fill the rest of the spawnpoints with normal items
        var NormalSpawnAmount = UnityEngine.Random.Range(Math.Max(0, maxItems / 3), Math.Max(0, maxItems));
        Debug.Log($"Spawned {NormalSpawnAmount} unstable items");
        while (i < NormalSpawnAmount)
        {
            var item = normalItems[i % normalItems.Length];
            var spawnpoint = spawnpoints[i % spawnpoints.Count];
            SpawnItem(item, spawnpoint);
            i++;
        }
        ItemSpawningFinishedRPC(foreignItemSpawnNumber, i);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ItemSpawningFinishedRPC(int foreignitemtotal, int itemTotal)
    {
        Debug.Log("it's " + foreignitemtotal);
        OnItemsSpawned.Invoke(foreignitemtotal, itemTotal);
    }

    [ContextMenu("Autofill")]
    private void Autofill()
    {
        if (idProvider == null)
        {
            Debug.LogError("No ItemIdProvider assigned to ItemManager");
        }
        if (level == null)
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

    private void SpawnItem(ItemData itemData, Transform spawnpoint, int stability = 0)
    {
        var item = Instantiate(itemData.Prefab, spawnpoint.position, spawnpoint.transform.rotation);
        if (item.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Spawn(true);
            SpawnedNetworkObjects.Add(networkObject);
            if (item.TryGetComponent(out Item itemComponent))
            {
                itemComponent.InStabilityWorth = stability;
            }
        }
        else
        {
            Debug.LogError("Item prefab does not have a NetworkObject component");
        }
    }
}