using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public interface IScanable
{
    string ScanTitle { get; }
    string ScanResult { get; }
    public void OnScan(Player player);
}
public class ScannerItem : Item
{
    [Header("Scanner Settings")]
    [SerializeField] private float scanRange = 10f;
    [Header("Scanner Refferences")]
    [SerializeField] private Canvas scannerCanvas;
    [SerializeField] private TMP_Text scanTitleText;
    [SerializeField] private TMP_Text scanResultText;
    [SerializeField] private ScrollRect scanResultScrollRect;
    [SerializeField] private LayerMask scanableLayer = ~0;


    public override void EnableVisuals(bool enable = true)
    {
        scannerCanvas.enabled = enable;
        base.EnableVisuals(enable);
    }

    Player player;
    IScanable scanable;
    public IScanable Scanable
    {
        private set
        {
            if (scanable == value) return;
            scanable = value;
            StopAllCoroutines();
            if (scanable != null)
            {
                scanTitleText.text = "?";
                scanResultText.text = "?";
            }
            else
            {
                scanTitleText.text = "";
                scanResultText.text = "";
            }
        }
        get
        {
            return scanable;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Scanable = null;
        scanTitleText.text = "";
        scanResultText.text = "";
    }
    public override void OnEquip(object character)
    {
        base.OnEquip(character);
        if (player == null && character is Player playerComponent)
        {
            player = playerComponent;
        }
        Scanable = null;
    }

    public override void OnEquipUpdate(object character)
    {
        base.OnEquipUpdate(character);
        if (isPickedUpByPlayer.Value && player != null)
        {
            var ray = new Ray(player.HeadTransform.position, player.HeadTransform.forward);
            if (Physics.Raycast(ray, out var hit, scanRange, scanableLayer, QueryTriggerInteraction.Ignore))
            {
                var scanable = hit.collider.GetComponent<IScanable>();
                Scanable = scanable;
            }
            if(player != null && player.IsLocalPlayer)
            {
                if (Player.Input.Player.Attack.WasPressedThisFrame())
                {
                    StartCoroutine(Scan());
                }
                if (Player.Input.Player.Attack.WasReleasedThisFrame())
                {
                    StopAllCoroutines();
                }
                if (Mouse.current.scroll.ReadValue().y != 0)
                {
                    scanResultScrollRect.verticalNormalizedPosition += Mouse.current.scroll.ReadValue().y * 0.1f;
                }
            }
        }
    }

    private IEnumerator Scan()
    {
        float scanTime = 1f;
        while (scanTime > 0)
        {
            scanTime -= Time.deltaTime;
            scanTitleText.text = $"Scanning {scanTime.ToString("F1")}s";
            scanResultText.text += ".";
            if(scanResultText.text.Length > 3) {
                scanResultText.text = "";
            }
            yield return null;
        }
        if (Scanable != null)
        {
            scanTitleText.text = Scanable.ScanTitle;
            scanResultText.text = Scanable.ScanResult;
            Scanable.OnScan(player);
        } else {
            scanTitleText.text = "Found Nothing";
            scanResultText.text = "";
        }
    }

    public override void OnUnequip(object character)
    {
        base.OnUnequip(character);
        player = null;
        Scanable = null;
    }
}