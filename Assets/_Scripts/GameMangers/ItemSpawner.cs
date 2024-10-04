using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    public ItemData[] _normalItems;
    public ItemData[] _forignItems;
    List<ItemData> objectSpawnList = new List<ItemData>();
    private int levelInstability;
    private int curInstabilityBudget;
    private int currentInstability;

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
            GetInstability();
            SpawnItems();
        }
    }

    private void SpawnItems()
    {
        int foreignItemCount = 0;
        int foreignItemIndex = 0;
        var spawnpoints = ItemSpawnPoint.GetShuffledSpawnPoints();
        int maxItems = spawnpoints.Count;

        while (curInstabilityBudget > 0)
        {
            // code to be modified
            if (currentInstability <= 0)
            {
                Debug.Log("Not enough tokens remaining to spawn an enemy");
                return;
            }
            ItemData itemToSpawn = ChooseObjectToSpawn();
            if (itemToSpawn == null)
            {
                Debug.LogWarning($"No enemies in spawnList");
                return;
            }
            int instabilityCost = UnityEngine.Random.Range(itemToSpawn.instabilityCostMin, itemToSpawn.instabilityCostMax);

            SpawnItem(itemToSpawn, spawnpoints[foreignItemIndex]);

            curInstabilityBudget -= instabilityCost;

            foreignItemIndex++;
            if (curInstabilityBudget == 0)
            {
                int normalItemsToSpawn = maxItems - foreignItemCount;
                int normalItemIndex = foreignItemIndex;
                for ( int i = 0; i < normalItemsToSpawn; i++ )
                {
                    SpawnItem(_normalItems[UnityEngine.Random.Range(0, _normalItems.Length)], spawnpoints[normalItemIndex]);
                    normalItemIndex++;
                }
            }
        }
    }

    private ItemData ChooseObjectToSpawn()
    {
        if (curInstabilityBudget == 0)
        {
            Debug.Log("No budget remaining");
            return null;
        }
        UpdateObjectList();

        if (objectSpawnList.Count == 0)
        {
            Debug.Log("No items can be spawned");
            return null;
        }

        int random = UnityEngine.Random.Range(0, objectSpawnList.Count);
        return objectSpawnList[random];
    }

    private void UpdateObjectList()
    {
        if (objectSpawnList.Count == 0 && curInstabilityBudget > 0)
        {
            objectSpawnList = new List<ItemData>();
            foreach (ItemData item in _forignItems)
            {
                ItemData newItem = item;
                if (newItem.instabilityCostMin <= curInstabilityBudget)
                {
                    if (newItem.instabilityCostMax > curInstabilityBudget)
                    {
                        newItem.instabilityCostMax = curInstabilityBudget;
                    }
                    objectSpawnList.Add(newItem);
                }
            }
            return;
        }

        foreach(ItemData item in objectSpawnList)
        {
            if (item.instabilityCostMin > curInstabilityBudget)
            {
                objectSpawnList.Remove(item);
                return;
            }

            if (item.instabilityCostMax > curInstabilityBudget)
            {
                item.instabilityCostMax = curInstabilityBudget;
            }
        }
    }

    private void GetItem()
    {
        int randomIndex = UnityEngine.Random.Range(0, _forignItems.Length);
        ItemData item = _forignItems[randomIndex];
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



    private void GetInstability()
    {
        levelInstability = InstabilityManager.instance.GetLevelInstability(InstabilityManager.instance.currentLevel);
        currentInstability = levelInstability;
        curInstabilityBudget = levelInstability;
    }
}