using System;
using Unity.Netcode;
public struct TimePeriodRefference : IEquatable<TimePeriodRefference>, INetworkSerializable
{
    int id;
    public TimePeriod Refference
    {
        get
        {
            if (id < 0)
            {
                return null;
            }
            return GameManager.ID.GetPeriodData(id);
        }
        set
        {
            if (value == null)
            {
                id = -1;
                return;
            }
            id = GameManager.ID.GetTimeId(value);
        }
    }

    public TimePeriodRefference(TimePeriod refference)
    {
        id = GameManager.ID.GetTimeId(refference);
    }
    public TimePeriodRefference(int id)
    {
        this.id = id;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
    }

    public bool Equals(TimePeriodRefference other) => other.id == id;
    public static implicit operator TimePeriodRefference(TimePeriod item) => new TimePeriodRefference(item);
    public static implicit operator TimePeriod(TimePeriodRefference refference) => refference.Refference;
}