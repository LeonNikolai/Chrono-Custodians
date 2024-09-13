public interface IEquippable {
    void Drop();
    void OnEquip(object playerCharacter);
    void OnEquipUpdate(object playerCharacter);
    void OnUnequip(object playerCharacter);
}