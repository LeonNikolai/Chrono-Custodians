using Unity.Netcode;
using UnityEngine;

public class AutoHost : MonoBehaviour
{
    void Awake()
    {
        NetworkManager.Singleton.StartHost();
    }
}