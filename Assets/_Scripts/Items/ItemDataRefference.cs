using System;
using Unity.Netcode;

public struct ItemDataRefference : IEquatable<ItemDataRefference>, INetworkSerializable
{
    int id;
    public ItemData Refference
    {
        get
        {
            if(id < 0)
            {
                return null;
            }
            return GameManager.ID.GetItemData(id);
        }
        set
        {
            if(value == null)
            {
                id = -1;
                return;
            }
            id = GameManager.ID.GetItemId(value);
        }
    }

    public ItemDataRefference(ItemData refference)
    {
        id = GameManager.ID.GetItemId(refference);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
    }

    public bool Equals(ItemDataRefference other) => other.id == id;
    public static implicit operator ItemDataRefference(ItemData item) => new ItemDataRefference(item);
    public static implicit operator ItemData(ItemDataRefference refference) => refference.Refference;
}