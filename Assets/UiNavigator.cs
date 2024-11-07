using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UiNavigator : NetworkBehaviour
{
    [SerializeField] private List<GameObject> UiElements = new List<GameObject>();

    [SerializeField] private NetworkVariable<int> currentIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentIndex.OnValueChanged += SetCurrentUI;
        if (currentIndex.Value != 0)
        {
            UiElements[0].SetActive(false);
            UiElements[currentIndex.Value].SetActive(true);
        }
    }


    public void SetVisibleUIElement(int index)
    {
        SwitchCurrentIndexRPC(index);
    }

    [Rpc(SendTo.Server)]
    private void SwitchCurrentIndexRPC(int index)
    {
        if (index > UiElements.Count - 1)
        {
            Debug.LogWarning("The index exceeds the element count. Tried: " + index + ". Exceeded: " + (UiElements.Count - 1));
        }
        currentIndex.Value = index;
    }

    private void SetCurrentUI(int previous, int current)
    {
        UiElements[previous].SetActive(false);
        UiElements[current].SetActive(true);
    }

}
