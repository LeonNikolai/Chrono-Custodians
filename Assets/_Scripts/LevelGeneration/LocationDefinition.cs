using System;
using System.Collections.Generic;
using UnityEngine;

public class LocationDefinition : MonoBehaviour
{
    [SerializeField] string _locationName;
    static Dictionary<string, LocationDefinition> teleportExitLocations = new Dictionary<string, LocationDefinition>();
    public static event Action<string> OnLocationChange = delegate { };
    public static LocationDefinition GetLocation(string locationName)
    {
        if (teleportExitLocations.ContainsKey(locationName))
        {
            return teleportExitLocations[locationName];
        }
        return null;
    }
    void OnEnable()
    {
        if (teleportExitLocations.ContainsKey(_locationName))
        {
            Debug.LogError("Teleport exit location with name " + _locationName + " already exists : " + teleportExitLocations[_locationName].gameObject.name);
            return;
        }
        teleportExitLocations.Add(_locationName, this);
        OnLocationChange.Invoke(_locationName);
    }

    void OnDisable()
    {
        if (teleportExitLocations.ContainsKey(_locationName))
        {
            teleportExitLocations.Remove(_locationName);
        }
    }
}
