using System;
using Unity.Netcode;

[System.Serializable]
public class LevelEndData : INetworkSerializable, IEquatable<LevelEndData>
{
    public int LevelId;
    public int RemainingUnstableItemStability;
    public int SendWrongInstability;
    public int SendCorrectInstability;
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
        RemainingUnstableItemStability = 0;
        SendWrongInstability = 0;
        SendCorrectInstability = 0;
    }
    public LevelEndData(LevelScene levelScene)
    {
        LevelId = LevelManager.GetSceneID(levelScene);
    }
    public LevelEndData(LevelScene levelScene, int RemainingUnstableItems, int WrongSendCount, int CorrectSendCount)
    {
        LevelId = LevelManager.GetSceneID(levelScene);
        this.RemainingUnstableItemStability = RemainingUnstableItems;
        this.SendWrongInstability = WrongSendCount;
        this.SendCorrectInstability = CorrectSendCount;
    }

    public LevelEndData(int level, int RemainingUnstableItems, int WrongSendCount, int CorrectSendCount)
    {
        this.LevelId = level;
        this.RemainingUnstableItemStability = RemainingUnstableItems;
        this.SendWrongInstability = WrongSendCount;
        this.SendCorrectInstability = CorrectSendCount;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref LevelId);
        serializer.SerializeValue(ref RemainingUnstableItemStability);
        serializer.SerializeValue(ref SendWrongInstability);
        serializer.SerializeValue(ref SendCorrectInstability);
        serializer.SerializeValue(ref Dayprogression);
    }

    public bool Equals(LevelEndData other)
    {
        if (LevelId != other.LevelId) return false;
        if (RemainingUnstableItemStability != other.RemainingUnstableItemStability) return false;
        if (SendWrongInstability != other.SendWrongInstability) return false;
        if (SendCorrectInstability != other.SendCorrectInstability) return false;
        if (Dayprogression != other.Dayprogression) return false;
        return true;
    }
}