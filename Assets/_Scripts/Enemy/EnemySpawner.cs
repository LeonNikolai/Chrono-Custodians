using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class NetworkedPrefabSpawnerWithTokens : NetworkBehaviour
{
    [SerializeField] private GameObject[] outsideEnemies; // The array of enemies that can be spawned outside
    [SerializeField] private GameObject[] insideEnemies; // The array of enemies that can be spawned inside
    private List<Vector3> outsideSpawnPoints; // This is an array of spawnpoints, this is using the waypoint manager.
    private List<Vector3> insideSpawnPoints; // This is an array of spawnpoints, this is using the waypoint manager.
    [SerializeField] private Difficulty difficulty; // Selected difficulty level

    [SerializeField] private int tokenCount;
    [SerializeField] private int tokensRemaining;
    [SerializeField] private int spawnTimeMin = 30;
    [SerializeField] private int spawnTimeMax = 60;

    private Dictionary<GameObject, int> spawnedEnemies = new Dictionary<GameObject, int>();

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public override void OnNetworkSpawn()
    {
        
        if (IsServer)
        {
            base.OnNetworkSpawn();
            outsideSpawnPoints = WaypointManager.instance.GetWaypoints(WaypointType.Outside);
            insideSpawnPoints = WaypointManager.instance.GetWaypoints(WaypointType.Inside);
            tokenCount = GetTokenCount(difficulty);
            StartCoroutine(SpawnTimer());
        }
    }

    // Get the number of tokens based on the selected difficulty
    private int GetTokenCount(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return 10;
            case Difficulty.Medium:
                return 15;
            case Difficulty.Hard:
                return 25;
            default:
                return 0;
        }
    }

    private int GetRemainingTokens()
    {
        if (spawnedEnemies.Count == 0) return tokenCount;
        int tokensRemaining = tokenCount;
        foreach (int value in spawnedEnemies.Values)
        {
            tokensRemaining -= value;
        }
        return tokensRemaining;
    }

    // Spawns the enemy if theres tokens available for it.
    private void SpawnEnemy(WaypointType type)
    {
        List<Vector3> enemyWaypoints = outsideSpawnPoints;
        if (type == WaypointType.Inside)
        {
            enemyWaypoints = insideSpawnPoints;
        }
        tokensRemaining = GetRemainingTokens();
        if (tokensRemaining <= 0)
        {
            Debug.Log("Not enough tokens remaining to spawn an enemy");
            return;
        }
        GameObject prefabToSpawn = ChooseEnemyToSpawn(type);
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"No enemies in spawnList");
            return;
        }
        int tokenCost = GetTokenCost(prefabToSpawn);


        if (tokenCost < tokensRemaining)
        {
            int randomSpawn = Random.Range(0, enemyWaypoints.Count);
            GameObject enemy = Instantiate(prefabToSpawn);
            tokensRemaining -= tokenCost;
            enemy.transform.position = enemyWaypoints[randomSpawn];
            spawnedEnemies.Add(enemy, tokenCost);
            if (enemy.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                // Spawn the networked object on the server
                networkObject.Spawn();
            }
            else
            {
                Debug.LogError("The enemy must have a NetworkObject component.");
            }
            enemy.GetComponent<Enemy>().SetWaypoints(enemyWaypoints);
        }
        else
        {
            Debug.Log("No available enemies that fit the remaining tokens.");
        }
    }

    private GameObject ChooseEnemyToSpawn(WaypointType type)
    {
        if (outsideEnemies.Length == 0)
        {
            Debug.Log("No enemy prefabs");
            return null;
        }

        List<GameObject> tempEnemiesList = new List<GameObject>();
        if (type == WaypointType.Outside)
        {
            tempEnemiesList = outsideEnemies.ToList();
            foreach (GameObject enemy in outsideEnemies)
            {
                if (enemy.GetComponent<Enemy>().tokenCost > tokensRemaining)
                {
                    tempEnemiesList.Remove(enemy);
                }
            }
        }
        else
        {
            tempEnemiesList = insideEnemies.ToList();
            foreach (GameObject enemy in insideEnemies)
            {
                if (enemy.GetComponent<Enemy>().tokenCost > tokensRemaining)
                {
                    tempEnemiesList.Remove(enemy);
                }
            }
        }

        if (tempEnemiesList.Count == 0)
        {
            Debug.Log("No enemies can be spawned");
            return null;
        }
        int random = Random.Range(0, tempEnemiesList.Count);
        return tempEnemiesList[random];
    }

    private int GetTokenCost(GameObject enemy)
    {
        int cost = enemy.GetComponent<Enemy>().tokenCost;
        return cost;
    }


    private IEnumerator SpawnTimer()
    {
        while (true)
        {

            int SpawnTime = Random.Range(spawnTimeMin, spawnTimeMax+1);
            yield return new WaitForSeconds(SpawnTime);
            CheckEnemies();
            SpawnEnemy(EnemyType());
        }
    }

    private WaypointType EnemyType()
    {
        int totalEnemies = outsideEnemies.Length + insideEnemies.Length;
        if (Random.Range(1, totalEnemies+1) <= outsideEnemies.Length)
        {
            return WaypointType.Outside;
        }
        return WaypointType.Inside;
    }

    private void CheckEnemies()
    {
        List<GameObject> enemiesToRemove = new List<GameObject>();

        foreach(GameObject enemy in spawnedEnemies.Keys)
        {

            if (enemy == null)
            {
                enemiesToRemove.Add(enemy);
            }
        }

        foreach (GameObject enemy in enemiesToRemove)
        {
            spawnedEnemies.Remove(enemy);
        }
    }
}
