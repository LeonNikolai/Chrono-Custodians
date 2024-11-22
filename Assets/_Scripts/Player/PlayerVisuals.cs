using System;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] MaterialReplacer replacer;
    [SerializeField] Animator animator;
    [SerializeField] TMP_Text usernameText;

    void Awake()
    {
        player.PlayerIsInMenu.OnValueChanged += OnPlayerIsInMenu;
        player.playerName.OnValueChanged += OnPlayerNameChanged;
        OnPlayerNameChanged(player.playerName.Value, player.playerName.Value);
    }

    private void OnPlayerNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        if(usernameText == null) {
            return;
        }
        usernameText.text = newValue.ToString();
    }

    void OnDestroy()
    {
        if(player != null) return;
        player.PlayerIsInMenu.OnValueChanged -= OnPlayerIsInMenu;
    }


    void  LateUpdate()
    {
        usernameText.transform.LookAt(Camera.main.transform);
        usernameText.transform.Rotate(0, 180, 0);
    }
    private void OnPlayerIsInMenu(bool previousValue, bool newValue)
    {
        replacer.Show = newValue;
    }
}