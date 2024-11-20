

using System;
using Unity.Netcode;

public struct LocationRenderingSettingsRefference : IEquatable<LocationRenderingSettingsRefference>, INetworkSerializable
{
    public static LocationRenderingSettingsRefference None = new LocationRenderingSettingsRefference(-1);
    int id;
    public LocationRenderingSettings Refference
    {
        get
        {
            if(id == -1)
            {
                return null;
            }
            return GameManager.ID.GetLocationRenderingSettings(id);
        }
        set
        {
            if (value == null)
            {
                id = -1;
                return;
            }
            id = GameManager.ID.GetRenderingSettingsId(value);
        }
    }


    public LocationRenderingSettingsRefference(LocationRenderingSettings refference)
    {
        id = GameManager.ID.GetRenderingSettingsId(refference);
    }
    public LocationRenderingSettingsRefference(int id)
    {
        this.id = id;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
    }

    public bool Equals(LocationRenderingSettingsRefference other) => other.id == id;
    public static implicit operator LocationRenderingSettingsRefference(LocationRenderingSettings item) => new LocationRenderingSettingsRefference(item);
    public static implicit operator LocationRenderingSettings(LocationRenderingSettingsRefference refference) => refference.Refference;
}