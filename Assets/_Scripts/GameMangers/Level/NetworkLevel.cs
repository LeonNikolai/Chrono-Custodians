using System;
using Unity.Netcode;


public struct NetworkLevel : INetworkSerializable, IEquatable<NetworkLevel>
{
    public int LevelId;
    public static NetworkLevel Default => new NetworkLevel(-1);
    public LevelScene Level
    {
        get
        {
            return LevelManager.GetSceneById(LevelId);
        }
        set
        {
            LevelId = LevelManager.GetSceneID(value);
        }
    }

    public NetworkLevel(int level)
    {
        LevelId = level;
    }
    public NetworkLevel(LevelScene level)
    {
        LevelId = LevelManager.GetSceneID(level);
    }
    public bool Equals(NetworkLevel other)
    {
        return LevelId == other.LevelId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref LevelId);
    }
}