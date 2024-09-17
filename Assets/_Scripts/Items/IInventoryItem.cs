public interface IInventoryItem
{
    ItemData ItemData { get; }
    void OnEnterInventory(object playerCharacter, ItemSlotType slotType);
    void OnExitInventory(object playerCharacter, ItemSlotType slotType);
}