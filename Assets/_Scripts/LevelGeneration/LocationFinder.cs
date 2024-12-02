using System;
using System.Collections.Generic;
using UnityEngine;

public class LocationFinder : MonoBehaviour
{
    [SerializeField] string locationName;
    void OnEnable()
    {
        Goto();
        LevelManager.instance.OnLevelLoaded.AddListener(LoadedLevel);
        LocationDefinition.OnLocationChange += OnAddedLocation;
    }

    private void OnAddedLocation(string location)
    {
        if (location == locationName) Goto();

    }

    private void LoadedLevel(LevelScene arg0)
    {
        Goto();
    }

    void OnDisable()
    {
        LevelManager.instance.OnLevelLoaded.RemoveListener(LoadedLevel);
    }

    private void Goto()
    {
        var location = LocationDefinition.GetLocation(locationName);
        if (location == null)
        {
            // Debug.LogError("Location with name " + locationName + " not found");
            return;
        }
        transform.position = location.transform.position;
        transform.rotation = location.transform.rotation;
    }
}
