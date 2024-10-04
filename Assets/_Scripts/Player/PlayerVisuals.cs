using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] MaterialReplacer replacer;

    void Awake()
    {
        player.PlayerIsInMenu.OnValueChanged += OnPlayerIsInMenu;
    }

    private void OnPlayerIsInMenu(bool previousValue, bool newValue)
    {
        replacer.Show = newValue;
    }
}