using System;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] MaterialReplacer replacer;
    [SerializeField] Animator animator;
    [SerializeField] TMP_Text[] usernameText;

    void Awake()
    {
        player.PlayerIsInMenu.OnValueChanged += OnPlayerIsInMenu;
        player.playerName.OnValueChanged += OnPlayerNameChanged;
        OnPlayerNameChanged(player.playerName.Value, player.playerName.Value);
        player.OnSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned()
    {
        OnPlayerNameChanged(player.playerName.Value, player.playerName.Value);
    }

    private void OnPlayerNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        if (usernameText == null)
        {
            return;
        }
        foreach(var text in usernameText)
        {
            text.gameObject.SetActive(true);
            text.text = newValue.ToString();
        }
    }

    void OnDestroy()
    {
        if (player != null) return;
        player.PlayerIsInMenu.OnValueChanged -= OnPlayerIsInMenu;
    }

    private void OnPlayerIsInMenu(bool previousValue, bool newValue)
    {
        replacer.Show = newValue;
    }
}