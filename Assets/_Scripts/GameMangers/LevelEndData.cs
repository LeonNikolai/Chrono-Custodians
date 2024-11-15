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
    public ItemSendEvent[] itemSendEvets;

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
        Dayprogression = 0;
        itemSendEvets = new ItemSendEvent[0];
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref LevelId);
        serializer.SerializeValue(ref RemainingUnstableItemStability);
        serializer.SerializeValue(ref SendWrongInstability);
        serializer.SerializeValue(ref SendCorrectInstability);
        serializer.SerializeValue(ref Dayprogression);
        serializer.SerializeValue(ref itemSendEvets);
    }

    public bool Equals(LevelEndData other)
    {
        if (LevelId != other.LevelId) return false;
        if (RemainingUnstableItemStability != other.RemainingUnstableItemStability) return false;
        if (SendWrongInstability != other.SendWrongInstability) return false;
        if (SendCorrectInstability != other.SendCorrectInstability) return false;
        if (Dayprogression != other.Dayprogression) return false;
        if(itemSendEvets.Length != other.itemSendEvets.Length) return false;
        return true;
    }
}