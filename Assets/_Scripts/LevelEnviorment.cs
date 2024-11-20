using Unity.Netcode;
using UnityEngine;

public class LevelEnviorment : NetworkBehaviour
{
    [SerializeField] LocationRenderingSettings ousideRenderingSettings;
    [SerializeField] LocationRenderingSettings insideRenderingSettings;
    public override void OnNetworkSpawn()
    {
        if (IsHost && GameManager.instance != null)
        {
            GameManager.instance._insideRenderingSettings.Value = insideRenderingSettings;
            GameManager.instance._outsideRenderingSettings.Value = ousideRenderingSettings;

        }
        base.OnNetworkSpawn();
    }
    public override void OnDestroy()
    {
        if (GameManager.instance != null && IsHost)
        {

            GameManager.instance._insideRenderingSettings.Value = LocationRenderingSettingsRefference.None;
            GameManager.instance._outsideRenderingSettings.Value = LocationRenderingSettingsRefference.None;
        }
        base.OnDestroy();
    }
}
