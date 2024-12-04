using System;
using Unity.Netcode;

[System.Serializable]
public class LevelEndData : INetworkSerializable, IEquatable<LevelEndData>
{
    public LevelSceneRefference Level;
    public int RemainingUnstableItemStability;
    public int SendWrongInstability;
    public int SendCorrectInstability;
    public int Dayprogression;
    public float OtherDecrease;
    public ItemSendEvent[] itemSendEvets;

    public LevelEndData()
    {
        Level = LevelSceneRefference.None;
        RemainingUnstableItemStability = 0;
        SendWrongInstability = 0;
        SendCorrectInstability = 0;
        Dayprogression = 0;
        itemSendEvets = new ItemSendEvent[0];
        OtherDecrease = 0;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Level);
        serializer.SerializeValue(ref RemainingUnstableItemStability);
        serializer.SerializeValue(ref SendWrongInstability);
        serializer.SerializeValue(ref SendCorrectInstability);
        serializer.SerializeValue(ref Dayprogression);
        serializer.SerializeValue(ref itemSendEvets);
        serializer.SerializeValue(ref OtherDecrease);
    }

    public bool Equals(LevelEndData other)
    {
        if (Level != other.Level) return false;
        if (RemainingUnstableItemStability != other.RemainingUnstableItemStability) return false;
        if (SendWrongInstability != other.SendWrongInstability) return false;
        if (SendCorrectInstability != other.SendCorrectInstability) return false;
        if (Dayprogression != other.Dayprogression) return false;
        if (itemSendEvets.Length != other.itemSendEvets.Length) return false;
        if(OtherDecrease != other.OtherDecrease) return false;
        return true;
    }
}