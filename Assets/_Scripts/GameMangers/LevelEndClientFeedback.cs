using System;
using Unity.Netcode;

[Serializable]
public struct NetworkLevelStabilty : INetworkSerializable, IEquatable<NetworkLevelStabilty>
{
    public NetworkLevel level;
    public int Stability;

    public bool Equals(NetworkLevelStabilty other)
    {
        return false;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref level);
        serializer.SerializeValue(ref Stability);
    }
}

[Serializable]
public class LevelEndClientFeedback : INetworkSerializable, IEquatable<LevelEndClientFeedback>
{
    public LevelEndData LevelEndData;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref LevelEndData);
    }
    
    public bool Equals(LevelEndClientFeedback other)
    {
        if (!LevelEndData.Equals(other.LevelEndData)) return false;
        return true;
    }
}