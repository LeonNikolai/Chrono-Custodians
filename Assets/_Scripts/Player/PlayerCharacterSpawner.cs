using Unity.Netcode;
using UnityEngine;

public class PlayerCharacterSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private Transform spawnPoint;

    public override void OnNetworkSpawn()
    {
        
    }

    private void SpawnPlayer(ulong  OwnerId)
    {
        if(IsServer)
        {
            var player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            player.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerId);
        }
    }
}