using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCheck : MonoBehaviour
{
    [SerializeField] bool _onlyLocalPlayer = false;
    [SerializeField] UnityEvent<bool> _playersDetected = new();

    public HashSet<Player> players = new();
    [SerializeField] bool isPlayerInside = false;
    public bool IsPlayerInside
    {
        get
        {
            return isPlayerInside;
        }
        set
        {
            if (isPlayerInside == value) return;
            Debug.Log("Player is inside: " + value);
            isPlayerInside = value;
            _playersDetected.Invoke(isPlayerInside);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.TryGetComponent(out Player player))
            {
                if (player == Player.LocalPlayer && _onlyLocalPlayer)
                {
                    players.Add(player);
                    Debug.Log("Player entered : " + players.Count);
                    IsPlayerInside = players.Count > 0;
                    return;
                }
                players.Add(player);
                IsPlayerInside = players.Count > 0;


            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Collision exit");
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.TryGetComponent(out Player player))
            {
                players.Remove(player);
                Debug.Log("Player exited : " + players.Count);
            }
        }
        IsPlayerInside = players.Count > 0;
    }

}
