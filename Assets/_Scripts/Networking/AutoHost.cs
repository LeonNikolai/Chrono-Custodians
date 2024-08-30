using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoHost : MonoBehaviour
{
    [Header("F1 = Host, F2 = Client")]
    [SerializeField] bool enableHostKeybind = false;
    [SerializeField] bool enableClientKeybind = false;
    void Awake()
    {

        if(NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if(NetworkManager.Singleton.IsListening)
        {
            return;
        }

        #if UNITY_EDITOR

        if(enableHostKeybind && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            NetworkManager.Singleton.StartHost();
        }
        if(enableClientKeybind && Keyboard.current.f2Key.wasPressedThisFrame)
        {
            NetworkManager.Singleton.StartClient();
        }
        #endif
    }
}