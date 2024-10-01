using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{

    public GameObject playerPrefab;
    public Transform[] spawnPoints = new Transform[0];
    public static Vector3 getSpawnPoint()
    {
        if(spawner) return spawner.getSpawnPos().position;
        return new Vector3(0, 0, 0);
    }
    static PlayerSpawner spawner;
    public override void OnNetworkSpawn()
    {
        spawner = this;
        Debug.Log("PlayerSpawner OnNetworkSpawn");
        if (IsServer || IsHost)
        {
            Debug.Log("PlayerSpawner OnNetworkSpawn Server");
            SpawnExistingPlayers();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void SpawnExistingPlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            OnClientConnected(client.ClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned in PlayerSpawner.");
            return;
        }
        if (Player.Players.ContainsKey(clientId)) return;
        Transform spawnPoint = getSpawnPos();
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        Debug.Log("Player spawned for client " + clientId);
    }

    private Transform getSpawnPos()
    {
        return spawnPoints.Length == 0 ? transform : spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    override public void OnDestroy()
    {
 
        if (IsServer)
        {
            if(NetworkManager.Singleton) NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}