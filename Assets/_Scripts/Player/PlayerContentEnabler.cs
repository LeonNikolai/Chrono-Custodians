using Unity.Netcode;
using UnityEngine;

public class PlayerContentEnabler : NetworkBehaviour
{
    [Header("Local Player Enabler")]
    [SerializeField] private GameObject[] enableGameObjectIfOwnedByLocal;
    [SerializeField] private Behaviour[] enableBehaviourIfOwnedByLocal;
    [Header("Local Player Disabler")]
    [SerializeField] private GameObject[] disableGameObjectIfOwnedByLocal;
    [SerializeField] private Behaviour[] disableBehaviourIfOwnedByLocal;

    [Header("Remote Player Content")]
    [SerializeField] private GameObject[] enableGameObjectIfOwnedRemotePlayer;
    [SerializeField] private Behaviour[] enableBehaviourIfOwnedRemotePlayer;
    [Header("Remote Player Disabler")]
    [SerializeField] private GameObject[] disableGameObjectIfOwnedRemotePlayer;
    [SerializeField] private Behaviour[] disableBehaviourIfOwnedRemotePlayer;


    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            LocalContent();
        }
        else
        {
            Other();
        }
        base.OnNetworkSpawn();
    }

    private void Other()
    {
        foreach (var go in enableGameObjectIfOwnedRemotePlayer)
        {
            go.SetActive(true);
        }
        foreach (var behaviour in enableBehaviourIfOwnedRemotePlayer)
        {
            behaviour.enabled = true;
        }
        foreach (var go in disableGameObjectIfOwnedRemotePlayer)
        {
            go.SetActive(false);
        }
        foreach (var behaviour in disableBehaviourIfOwnedRemotePlayer)
        {
            behaviour.enabled = false;
        }
    }

    private void LocalContent()
    {
        foreach (var go in enableGameObjectIfOwnedByLocal)
        {
            go.SetActive(true);
        }
        foreach (var behaviour in enableBehaviourIfOwnedByLocal)
        {
            behaviour.enabled = true;
        }
        foreach (var go in disableGameObjectIfOwnedByLocal)
        {
            go.SetActive(false);
        }
        foreach (var behaviour in disableBehaviourIfOwnedByLocal)
        {
            behaviour.enabled = false;
        }
    }
}