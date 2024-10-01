
using UnityEngine;

public interface IEquippable {
    void Drop(Vector3? position = null);
    void OnEquip(object equipper);
    void OnEquipUpdate(object equipper);
    void OnUnequip(object equipper);
}