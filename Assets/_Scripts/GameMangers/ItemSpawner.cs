using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
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
            SpawnItems();
        }
    }

    private void SpawnItems()
    {
        int forignItems = CalulateForignItemAmount();
        int normalItemIndex = 0;
        int forignItemIndex = 0;
        var spawnpoints = ItemSpawnPoint.GetShuffledSpawnPoints();
        foreach (var spawnpoint in spawnpoints)
        {
            if (forignItems > 0)
            {
                SpawnItem(_forignItems[forignItemIndex], spawnpoint);
                forignItemIndex++;
                forignItems--;
            }
            else
            {
                SpawnItem(_normalItems[normalItemIndex], spawnpoint);
                normalItemIndex++;
            }
        }
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

    private int CalulateForignItemAmount()
    {
        float TimeStabilityMultiplier = Game.TimeStability / 100;
        return ItemSpawnPoint.AllSpawnPoints.Count / 2;
    }
}