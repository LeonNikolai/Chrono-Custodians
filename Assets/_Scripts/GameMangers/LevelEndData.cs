using System;
using Unity.Netcode;

[System.Serializable]
public class LevelEndData : INetworkSerializable, IEquatable<LevelEndData>
{
    public int LevelId;
    public int RemainingUnstableItems;
    public int WrongSendCount;
    public int CorrectSendCount;
    public int Dayprogression;

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
    public LevelEndData()
    {
        LevelId = -1;
        RemainingUnstableItems = 0;
        WrongSendCount = 0;
        CorrectSendCount = 0;
    }
    public LevelEndData(LevelScene levelScene)
    {
        LevelId = LevelManager.GetSceneID(levelScene);
    }
    public LevelEndData(LevelScene levelScene, int RemainingUnstableItems, int WrongSendCount, int CorrectSendCount)
    {
        LevelId = LevelManager.GetSceneID(levelScene);
        this.RemainingUnstableItems = RemainingUnstableItems;
        this.WrongSendCount = WrongSendCount;
        this.CorrectSendCount = CorrectSendCount;
    }

    public LevelEndData(int level, int RemainingUnstableItems, int WrongSendCount, int CorrectSendCount)
    {
        this.LevelId = level;
        this.RemainingUnstableItems = RemainingUnstableItems;
        this.WrongSendCount = WrongSendCount;
        this.CorrectSendCount = CorrectSendCount;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref LevelId);
        serializer.SerializeValue(ref RemainingUnstableItems);
        serializer.SerializeValue(ref WrongSendCount);
        serializer.SerializeValue(ref CorrectSendCount);
        serializer.SerializeValue(ref Dayprogression);
    }

    public bool Equals(LevelEndData other)
    {
        if (LevelId != other.LevelId) return false;
        if (RemainingUnstableItems != other.RemainingUnstableItems) return false;
        if (WrongSendCount != other.WrongSendCount) return false;
        if (CorrectSendCount != other.CorrectSendCount) return false;
        if (Dayprogression != other.Dayprogression) return false;
        return true;
    }
}