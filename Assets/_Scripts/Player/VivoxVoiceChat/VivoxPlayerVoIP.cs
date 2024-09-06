using UnityEngine;
using UnityEngine.UI;

public class VivoxPlayerVoIP : MonoBehaviour
{
    [SerializeField] Toggle uiVoiceToggle;
    [SerializeField] GameObject thisPlayerHead;

    private void Start()
    {
        VivoxPlayer VP = GetComponent<VivoxPlayer>();
        // Add some toggle here later
        VP.LoginToVivoxAsync();

        VP.setPlayerHeadPos(thisPlayerHead);
    }
}
