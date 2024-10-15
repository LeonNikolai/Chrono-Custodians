using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;
public interface IScanable
{
    string ScanTitle { get; }
    string ScanResult { get; }
    public void OnScan(Player player);
}
public class ScannerItem : Item, ItemUseToolTip
{
    [Header("Scanner Settings")]
    [SerializeField] private float scanRange = 10f;
    [SerializeField] private bool minigameMustScanInOrder = false;


    [Header("Scanner Refferences")]
    [SerializeField] private Canvas scannerCanvas;
    [SerializeField] private TMP_Text scanTitleText;
    [SerializeField] private TMP_Text scanResultText;
    [SerializeField] private TMP_Text lookAtScanableText;
    [SerializeField] private ScrollRect scanResultScrollRect;
    [SerializeField] private LayerMask scanableLayer = ~0;
    [SerializeField] ScannerLight scannerLight1;
    [SerializeField] ScannerLight scannerLight2;
    [SerializeField] ScannerLight scannerLight3;



    public override void EnableVisuals(bool enable = true)
    {
        scannerCanvas.enabled = enable;
        base.EnableVisuals(enable);
    }

    Player player;
    IScanable lookAtScanable;
    public IScanable LookAtScanable
    {
        private set
        {
            if (lookAtScanable == value) return;
            lookAtScanable = value;
            UpdateVisuals();
        }
        get
        {
            return lookAtScanable;
        }
    }
    IScanable currentlyScanning = null;
    IScanable previosSuccessfullScannedItem = null;
    public IScanable CurrentlyScanning
    {
        private set
        {
            if (currentlyScanning == value) return;
            currentlyScanning = value;
            if (value != null) previosSuccessfullScannedItem = value;
            UpdateVisuals();
        }
        get
        {
            return currentlyScanning;
        }
    }

    private void UpdateVisuals()
    {
        if (lookAtScanable == null)
        {
            lookAtScanableText.text = "";

            scannerLight1.SetState(false);
            scannerLight2.SetState(false);
            scannerLight3.SetState(false);
            scanTitleText.color = new Color(1, 1, 1, 0.2f);
            return;
        }
        if (LookAtScanable != CurrentlyScanning && LookAtScanable != previosSuccessfullScannedItem)
        {
            lookAtScanableText.text = "?";
            scanTitleText.color = new Color(1, 1, 1, 0.05f);
            scannerLight1.SetState(false);
            scannerLight2.SetState(true);
            scannerLight3.SetState(false);
        }
        else if (LookAtScanable == previosSuccessfullScannedItem)
        {
            scannerLight1.SetState(true);
            scannerLight2.SetState(true);
            scannerLight3.SetState(true);
            lookAtScanableText.text = "";
            scanTitleText.color = new Color(1, 1, 1, 1);
        }
        else
        {
            scannerLight1.SetState(false);
            scannerLight2.SetState(false);
            scannerLight3.SetState(false);
            lookAtScanableText.text = "";
            scanTitleText.color = new Color(1, 1, 1, 1);
        }
    }

    public string ItemToolTip => $"Hold {Player.Input?.Player.UseItemPrimary?.activeControl?.displayName ?? "Left Mouse"} to scan objects, Mouse Wheel to scroll results";

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        LookAtScanable = null;
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
        LookAtScanable = null;
    }

    public Coroutine activeScanCoroutine;
    public float lastScanTime;
    public const float scanCooldown = 0.1f;
    public override void OnEquipUpdate(object character)
    {
        base.OnEquipUpdate(character);
        if (isPickedUpByPlayer.Value && player != null)
        {
            var ray = new Ray(player.HeadTransform.position + player.HeadTransform.forward * 0.25f, player.HeadTransform.forward);
            Debug.DrawRay(ray.origin, ray.direction * scanRange, Color.red, 0.1f);
            if (Physics.Raycast(ray, out var hit, scanRange, scanableLayer, QueryTriggerInteraction.Collide))
            {
                var scanable = hit.collider.GetComponent<IScanable>();
                LookAtScanable = scanable;
            }
            if (player != null && player.IsLocalPlayer)
            {
                if (CurrentlyScanning == null)
                {
                    if (Player.Input.Player.UseItemPrimary.WasPressedThisFrame() && Time.time - lastScanTime > scanCooldown)
                    {

                        lastScanTime = Time.time;
                        CurrentlyScanning = LookAtScanable;
                        if (activeScanCoroutine != null)
                        {
                            Debug.Log("Stopping Coroutine");
                            StopCoroutine(activeScanCoroutine);
                        }
                        activeScanCoroutine = StartCoroutine(Scan());
                    }
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
            if (Player.Input.Player.UseItemPrimary.IsPressed() == false)
            {
                scanTitleText.text = "Cancelled";
                scanResultText.text = "Hold ItemAction (Left Mouse) to start scanning";
                CurrentlyScanning = null;
                scannerLight1.SetState(false);
                scannerLight2.SetState(false);
                scannerLight3.SetState(false);
                Debug.Log("Scanning Cancelled");
                yield break;
            }
            scanTime -= Time.deltaTime;
            scanTitleText.text = $"Scanning {scanTime.ToString("F1")}s";
            int dots = Mathf.FloorToInt(scanTime * 4 * 4);

            scannerLight3.SetState(dots % 4 == 0);
            scannerLight1.SetState(dots % 4 == 1);
            scannerLight2.SetState(dots % 4 == 2);
            scanResultText.text += ".";
            if (scanResultText.text.Length > 3)
            {
                scanResultText.text = "";
            }

            yield return null;
        }
        if (CurrentlyScanning == null)
        {
            scanTitleText.text = "Air";
            scanResultText.text = "There was nothing there";
            CurrentlyScanning = null;
            yield break;
        }
        if (CurrentlyScanning != LookAtScanable)
        {
            scanTitleText.text = "Unsuccesfull Scan";
            scanResultText.text = "Try not looking away";
            CurrentlyScanning = null;
            yield break;
        }
        if (CurrentlyScanning == null)
        {
            scanTitleText.text = "Nothing to scan";
            scanResultText.text = "";
            CurrentlyScanning = null;
            yield break;
        }

        scannerLight1.SetState(true);
        scannerLight2.SetState(true);
        scannerLight3.SetState(true);
        scanTitleText.text = LookAtScanable.ScanTitle;
        scanResultText.text = LookAtScanable.ScanResult;

        if (CurrentlyScanning is Item item)
        {
            if (item._requiresMinigameToScan == MinigameType.PointScanningWorld && item.HasBeenScanned.Value == false)
            {
                yield return StartCoroutine(ItemMinigame());
                yield break;
            }
            else
            {
                CurrentlyScanning.OnScan(player);
                if (item.ItemData.ScanMinigameResults != null && item.ItemData.ScanMinigameResults.Length > 0)
                {
                    scanResultText.text = "";
                    ListScannedMinigameResults(item.ItemData.ScanMinigameResults, item.ItemData.ScanMinigameResults.Length);
                }
                else
                {
                    scanResultText.text = item.ScanResult;
                }
                Hud.ScannerNotification = scanResultText.text;
            }
        }
        else
        {
            CurrentlyScanning.OnScan(player);
            Hud.ScannerNotification = CurrentlyScanning.ScanResult;
        }
        CurrentlyScanning = null;
    }

    private IEnumerator ItemMinigame()
    {
        if (CurrentlyScanning is not Item currentItem)
        {
            CurrentlyScanning = null;
            yield break;
        }
        ItemData data = currentItem.ItemData;
        var results = data.ScanMinigameResults;
        ItemScannerPoint[] targetPoints = ItemScannerPoint.GetRandom(results.Length, player?.Location ?? LocationType.Outside);
        int scannedCount = 0;
        foreach (var item in targetPoints)
        {
            item.Activate();
        }
        RenderItemMinigameText(results, targetPoints, scannedCount);
        var ray = new Ray(player.HeadTransform.position + player.HeadTransform.forward * 0.25f, player.HeadTransform.forward);
        Hud.ScannerNotification = $"Scanned {scannedCount}/{targetPoints.Length}";
        while (scannedCount < targetPoints.Length)
        {
            float scanTime = 1f;
            bool isScanning = false;
            while (scanTime > 0)
            {
                RenderItemMinigameText(results, targetPoints, scannedCount);
                if (!isScanning)
                {
                    if (Player.Input.Player.UseItemPrimary.WasPressedThisFrame())
                    {
                        isScanning = true;
                    }
                    yield return null;
                    continue;
                }
                if (Player.Input.Player.UseItemPrimary.WasReleasedThisFrame())
                {
                    scanTime = 1f;
                    isScanning = false;
                    scanTitleText.text = currentItem.ScanTitle;
                    yield return null;
                    continue;
                }

                scanTime -= Time.deltaTime;
                scanTitleText.text = $"Scanning {scanTime.ToString("F1")}s";
                int dots = Mathf.FloorToInt(scanTime * 4);
                scanResultText.text += ".";
                if (scanResultText.text.Length > 3)
                {
                    scanResultText.text = "";
                }
                yield return null;
            }

            if (targetPoints.Contains(LookAtScanable) && LookAtScanable is ItemScannerPoint point)
            {
                if (minigameMustScanInOrder && targetPoints[scannedCount] != point)
                {
                    CurrentlyScanning = null;
                    scanTitleText.text = "Unsuccesfull Scan";
                    scanResultText.text = "Points need to be scanned in order";
                    yield break;
                }
                var resultsCopy = results[scannedCount];
                string result = resultsCopy != null ? !resultsCopy.IsEmpty ? resultsCopy.GetLocalizedString() : "?" : "?";
                scannedCount++;
                Hud.ScannerNotification = $"Scanned {scannedCount}/{targetPoints.Length} points \n {result}";
                RenderItemMinigameText(results, targetPoints, scannedCount);
                point.Deactivate();

            }
            else if (LookAtScanable is not null)
            {
                CurrentlyScanning = null;
                scanTitleText.text = "Unsuccesfull Scan";
                scanResultText.text = "Start scanning something else";
                yield break;
            }
            yield return null;
        }

        foreach (var item in targetPoints)
        {
            item.Deactivate();
        }

        scanTitleText.text = currentItem.ScanTitle;
        scanResultText.text = "";
        ListScannedMinigameResults(results, scannedCount);
        Hud.ScannerNotification = scanResultText.text;
        CurrentlyScanning?.OnScan(player);
        CurrentlyScanning = null;
    }

    private void RenderItemMinigameText(LocalizedString[] results, ItemScannerPoint[] targetPoints, int scannedCount)
    {
        scanResultText.text = $"This scan requires more information, scan nearby points {scannedCount}/{targetPoints.Length} points\n";
        ListScannedMinigameResults(results, scannedCount);
    }

    private void ListScannedMinigameResults(LocalizedString[] results, int scannedCount)
    {
        for (int i = 0; i < scannedCount; i++)
        {
            scanResultText.text += "\n" + (results[i] != null ? !results[i].IsEmpty ? results[i].GetLocalizedString() : "?" : "?");
        }
    }

    public override void OnUnequip(object character)
    {
        base.OnUnequip(character);
        player = null;
        LookAtScanable = null;
    }
}