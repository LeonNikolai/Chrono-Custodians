using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.VFX;
using System;

public class TeleportItem : Item, ItemUseToolTip
{
    Player player;
    private List<Player> players = new List<Player>();
    private int selectedPlayer = 0;
    private NetworkVariable<int> remainingUses = new NetworkVariable<int>(4, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private int SelectedPlayer
    {
        get => selectedPlayer;
        set
        {
            // if (value != selectedPlayer) return;
            foreach (var item in playerCards)
            {
                if (item == null) continue;
                item.color = unselectedColor;
            }
            if (value < 0)
            {
                selectedPlayer = players.Count - 1;
            }
            else if (value >= players.Count)
            {
                selectedPlayer = 0;
            }
            else
            {
                selectedPlayer = value;
                selectedPlayer = selectedPlayer % players.Count;
            }
            playerCards[selectedPlayer].color = selectedColor;
        }
    }
    [SerializeField] private Transform contentArea;
    [SerializeField] private GameObject playerCardPrefab;
    [SerializeField] private GameObject teleportingPlayerPrefab;
    [SerializeField] private GameObject teleportPointPrefab;
    private List<Image> playerCards = new List<Image>();

    [Header("UI")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject selection;

    [SerializeField] private GameObject actionInProgress;
    [SerializeField] private TMP_Text teleportText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;




    private bool teleporterDisabled = false;

    private float curCooldown;
    [SerializeField] private float cooldown;

    [SerializeField] private Material chargeOff;
    [SerializeField] private Material chargeOn;
    [SerializeField] private MeshRenderer[] chargeIndicators;


    public string ItemToolTip => $"Press {Player.Input?.Player.UseItemPrimary?.activeControl?.displayName ?? "Left Mouse"} to teleport the selected player, Mouse Wheel to scroll between players";

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        remainingUses.OnValueChanged += UpdateVisuals;
        canvas.SetActive(false);
        if (!IsServer) return;
        GameManager.DestroyCurrentLevel += ResetDevice;
    }

    private void UpdateVisuals(int previousValue, int newValue)
    {
        for (int i = 0; i < chargeIndicators.Length; i++)
        {
            var materials = chargeIndicators[i].materials;
            materials[1] = i < newValue ? chargeOn : chargeOff;
            chargeIndicators[i].materials = materials;
        }
    }

    private void ResetDevice()
    {
        if (IsServer)
        {
            remainingUses.Value = 4;
        }
    }



    public override void OnEquip(object character)
    {
        base.OnEquip(character);
        canvas.SetActive(true);


        if (player == null && character is Player playerComponent)
        {
            player = playerComponent;
        }
        UpdateView();
    }

    private void UpdateView()
    {
        players.Clear();
        foreach (var _player in Player.AllPlayers)
        {
            players.Add(_player);
        }

        if (playerCards.Count != Player.AllPlayers.Count && playerCards.Count > 0)
        {
            foreach (var card in playerCards)
            {
                Destroy(card);
            }
            playerCards.Clear();
        }
        if (playerCards.Count == 0)
        {
            for (int i = 0; i < Player.AllPlayers.Count; i++)
            {
                GameObject go = Instantiate(playerCardPrefab, contentArea);
                playerCards.Add(go.GetComponent<Image>());
                go.GetComponentInChildren<TMP_Text>().text = players[i].PlayerName;
                playerCards[i].color = i == SelectedPlayer ? selectedColor : unselectedColor;
            }
        }

    }

    public override void OnUnequip(object character)
    {
        base.OnUnequip(character);
        canvas.SetActive(false);
    }

    public override void OnEquipUpdate(object character)
    {
        base.OnEquipUpdate(character);

        if (playerCards.Count != Player.AllPlayers.Count)
        {
            UpdateView();
        }

        if (player != null && player.IsOwner)
        {
            if (Mouse.current.scroll.ReadValue().y > 0)
            {
                if (SelectedPlayer < Player.AllPlayers.Count - 1)
                {
                    SelectedPlayer++;
                }
            }
            else if (Mouse.current.scroll.ReadValue().y < 0)
            {
                if (selectedPlayer > 0)
                {
                    SelectedPlayer--;
                }
            }

            if (teleporterDisabled) return;

            if (Mouse.current.leftButton.wasPressedThisFrame && remainingUses.Value > 0)
            {
                CalculateChargesRPC();
                StartCoroutine(TeleportPlayer());
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void CalculateChargesRPC()
    {
        remainingUses.Value--;
    }



    private IEnumerator TeleportPlayer()
    {
        selection.SetActive(false);
        actionInProgress.SetActive(true);
        teleporterDisabled = true;
        Vector3 teleportPos = player.transform.position;
        LocationType locationType = player.Location;

        SpawnEffectsRPC(teleportPos, players[selectedPlayer].OwnerClientId);
        teleportText.text = "Teleporting " + players[selectedPlayer].PlayerName + " in";
        countdownText.text = "3";
        // Teleport after the wait here
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        yield return new WaitForSeconds(1);
        countdownText.text = "1";
        yield return new WaitForSeconds(1);


        players[selectedPlayer].GetComponent<PlayerMovement>().TeleportRpc(teleportPos, locationType);

        curCooldown = cooldown;
        teleportText.text = "Teleporter ready in";
        while (curCooldown > 0)
        {
            countdownText.text = Mathf.Ceil(curCooldown).ToString();
            yield return new WaitForEndOfFrame();
            curCooldown -= Time.deltaTime;
        }
        selection.SetActive(true);
        actionInProgress.SetActive(false);
        teleporterDisabled = false;
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnEffectsRPC(Vector3 teleportPos, ulong clientId)
    {
        if (Player.Players.TryGetValue(clientId, out Player targetPlayer))
        {
            StartCoroutine(SpawnEffects(teleportPos, targetPlayer));
        }
    }

    private IEnumerator SpawnEffects(Vector3 teleportPos, Player targetPlayer)
    {
        GameObject locationEffect = Instantiate(teleportPointPrefab);
        locationEffect.transform.position = teleportPos;
        GameObject playerEffect = Instantiate(teleportingPlayerPrefab, targetPlayer.transform);

        yield return new WaitForSeconds(2.99f);
        playerEffect.transform.parent = null;

        locationEffect.GetComponent<VisualEffect>().Stop();
        playerEffect.GetComponent<VisualEffect>().Stop();
        Destroy(locationEffect, 2);
        Destroy(playerEffect, 2);
    }


    private void ResetTeleporter(bool isTeleporterEnabled)
    {
        StopAllCoroutines();
        teleporterDisabled = isTeleporterEnabled;
    }
}
