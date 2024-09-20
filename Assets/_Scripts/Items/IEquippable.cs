
using UnityEngine;

public interface IEquippable {
    void Drop(Vector3? position = null);
    void OnEquip(object playerCharacter);
    void OnEquipUpdate(object playerCharacter);
    void OnUnequip(object playerCharacter);
}