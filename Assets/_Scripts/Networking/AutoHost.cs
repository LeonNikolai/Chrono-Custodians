using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoHost : MonoBehaviour
{
    [Header("F1 = Host, F2 = Client")]
    [SerializeField] bool enableHostKeybind = false;
    [SerializeField] bool enableClientKeybind = false;
    private Rect buttonRect1 = new Rect(10, 10, 150, 50); // Position and size for Button 1
    private Rect buttonRect2 = new Rect(10, 70, 150, 50); // Position and size for Button 2 (moved down)


    void Start()
    {

        if (NetworkManager.Singleton && NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            return;
        }

#if UNITY_EDITOR

        if (enableHostKeybind && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            NetworkManager.Singleton.StartHost();
        }
        if (enableClientKeybind && Keyboard.current.f2Key.wasPressedThisFrame)
        {
            NetworkManager.Singleton.StartClient();
        }


#endif
    }
    void OnGUI()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            return;
        }

        if (GUI.Button(buttonRect1, "Host"))
        {
            NetworkManager.Singleton.StartHost();
        }

        // Draw the second button
        if (GUI.Button(buttonRect2, "Join"))
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}