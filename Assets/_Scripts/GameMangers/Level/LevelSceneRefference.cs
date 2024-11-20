using System;
using Unity.Netcode;


public struct LevelSceneRefference : IEquatable<LevelSceneRefference>, INetworkSerializable
{
    int id;
    public LevelScene Refference
    {
        get
        {
            return GameManager.ID.GetLevelScene(id);
        }
        set
        {
            if (value == null)
            {
                id = -1;
                return;
            }
            id = GameManager.ID.GetLevelSceneId(value);
        }
    }

    public static LevelSceneRefference None = new LevelSceneRefference(-1);

    public LevelSceneRefference(int id = -1)
    {
        this.id = id;
    }
    public LevelSceneRefference(LevelScene refference)
    {
        id = GameManager.ID.GetLevelSceneId(refference);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
    }

    public bool Equals(LevelSceneRefference other) => other.id == id;

    public static implicit operator LevelSceneRefference(LevelScene scene) => new LevelSceneRefference(scene);
    public static implicit operator LevelScene(LevelSceneRefference refference) => refference.Refference;
    public static bool operator ==(LevelSceneRefference a, LevelSceneRefference b) => a.id == b.id;
    public static bool operator !=(LevelSceneRefference a, LevelSceneRefference b) => a.id != b.id;
}
