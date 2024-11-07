using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Chrono Custodians/New Time Period")]
public class TimePeriod : ScriptableObject, INetworkSerializable
{
    public string periodName;
    public string description;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref periodName);
        serializer.SerializeValue(ref description);
    }
}
