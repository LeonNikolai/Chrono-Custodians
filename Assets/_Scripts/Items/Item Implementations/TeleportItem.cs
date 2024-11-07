using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.VFX;

public class TeleportItem : Item, ItemUseToolTip
{
    Player player;
    private List<Player> players = new List<Player>();
    private int selectedPlayer;
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


    public string ItemToolTip => $"Hold {Player.Input?.Player.UseItemPrimary?.activeControl?.displayName ?? "Left Mouse"} to teleport the selected (blue) player, Mouse Wheel to scroll between players";

    public override void OnEquip(object character)
    {
        base.OnEquip(character);
        canvas.SetActive(true);
        if (player == null && character is Player playerComponent)
        {
            player = playerComponent;
        }
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
                go.GetComponentInChildren<TMP_Text>().text = player.GetPlayerName() + player.GetComponent<NetworkObject>().NetworkObjectId;
                if (i == selectedPlayer)
                {
                    playerCards[i].color = selectedColor;
                }
                else
                {
                    playerCards[i].color = unselectedColor;
                }
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
            if (teleporterDisabled) return;
            if (Mouse.current.scroll.ReadValue().y > 0)
            {
                if (selectedPlayer < Player.AllPlayers.Count - 1)
                {
                    playerCards[selectedPlayer].color = unselectedColor;
                    selectedPlayer++;
                    playerCards[selectedPlayer].color = selectedColor;
                }
            }
            else if (Mouse.current.scroll.ReadValue().y < 0)
            {
                if (selectedPlayer > 0)
                {
                    playerCards[selectedPlayer].color = unselectedColor;
                    selectedPlayer--;
                    playerCards[selectedPlayer].color = selectedColor;
                }
            }

            if (Mouse.current.leftButton.isPressed)
            {
                StartCoroutine(TeleportPlayer());
            }
        }
    }



    private IEnumerator TeleportPlayer()
    {
        selection.SetActive(false);
        actionInProgress.SetActive(true);
        teleporterDisabled = true;
        Vector3 teleportPos = player.transform.position;

        SpawnEffectsRPC(teleportPos);
        teleportText.text = "Teleporting " + players[selectedPlayer].playerName + " in";
        countdownText.text = "3";
        // Teleport after the wait here
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        yield return new WaitForSeconds(1);
        countdownText.text = "1";
        yield return new WaitForSeconds(1);


        players[selectedPlayer].GetComponent<PlayerMovement>().TeleportRpc(teleportPos);

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
    private void SpawnEffectsRPC(Vector3 teleportPos)
    {
        StartCoroutine(SpawnEffects(teleportPos));
    }

    private IEnumerator SpawnEffects(Vector3 teleportPos)
    {
        GameObject locationEffect = Instantiate(teleportPointPrefab);
        locationEffect.transform.position = teleportPos;
        GameObject playerEffect = Instantiate(teleportingPlayerPrefab, players[selectedPlayer].transform);

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
