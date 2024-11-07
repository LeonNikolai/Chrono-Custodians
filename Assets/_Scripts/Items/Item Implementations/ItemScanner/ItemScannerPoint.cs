using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemScannerPoint : MonoBehaviour, IScanable, IInteractionMessage, IInteractable
{
    public Collider[] colliders;
    public Renderer[] visuals;

    public static Dictionary<LocationType, List<ItemScannerPoint>> pointGroups = new Dictionary<LocationType, List<ItemScannerPoint>>();
    public static Action<ItemScannerPoint> OnPointScan = delegate { };

    public LocationType group = LocationType.Outside;

    public string ScanTitle => "Scanner Point";

    public string ScanResult => "A Scanner Point";

    public string InteractionMessage => "Scan with Scanner";

    public string CantInteractMessage => "Scan with Scanner";
    public bool Interactable => false;

    public static ItemScannerPoint[] GetRandom(int amount, Vector3 playerLocation, LocationType group)
    {
        if (amount <= 0)
        {
            return new ItemScannerPoint[0];
        }
        if (pointGroups.TryGetValue(group, out List<ItemScannerPoint> points))
        {
            return points.OrderBy(x => Vector3.Distance(x.transform.position, playerLocation)).Take(amount).ToArray();
        }
        return new ItemScannerPoint[0];
    }
    public bool IsActive { get; private set; }
    public void Activate()
    {
        IsActive = true;
    }

    public void ShowPoint()
    {
        SetColliders(true);

        SetVisuals(true);
    }

    internal void Deactivate()
    {
        IsActive = false;
        SetColliders(false);
        SetVisuals(false);
    }
    private void SetColliders(bool v)
    {
        foreach (var collider in colliders)
        {
            if (collider == null) continue;
            collider.enabled = v;
        }
    }

    private void SetVisuals(bool v)
    {
        foreach (var visual in visuals)
        {
            if (visual == null) continue;
            visual.enabled = v;
        }
    }

    private void Awake()
    {
        if (!pointGroups.ContainsKey(group))
        {
            pointGroups.Add(group, new List<ItemScannerPoint>());
        }
        pointGroups[group].Add(this);
        Deactivate();
    }


    void OnDestroy()
    {
        if (pointGroups.ContainsKey(group))
        {
            pointGroups[group].Remove(this);
        }
    }

    public void OnScan(Player player)
    {
        OnPointScan.Invoke(this);
    }

    public void Interact(Player player)
    {

    }
}