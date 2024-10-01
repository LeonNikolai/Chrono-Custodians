public interface IInventoryItem
{
    ItemData ItemData { get; }
    void OnEnterInventory(object inventory, ItemSlotType slotType);
    void OnExitInventory(object inventory, ItemSlotType slotType);
}