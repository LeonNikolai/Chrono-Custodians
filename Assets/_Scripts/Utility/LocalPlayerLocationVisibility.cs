using System;
using UnityEngine;

public class LocalPlayerLocationVisibility : MonoBehaviour
{
    [SerializeField] LocationType locationType;
    [SerializeField] GameObject[] ObjectsToShowIfInLocation;
    [SerializeField] bool inverted;

    private void Awake()
    {
        Player.RenderChanged += OnRenderChange;
    }
    void OnDestroy()
    {
        Player.RenderChanged -= OnRenderChange;
    }

    private void OnRenderChange()
    {
        if (Player.LocalPlayer == null) return;
        if (inverted)
        {
            foreach (var go in ObjectsToShowIfInLocation)
            {
                go.SetActive(Player.LocalPlayer.Location != locationType);
            }
        }
        else
        {
            foreach (var go in ObjectsToShowIfInLocation)
            {
                go.SetActive(Player.LocalPlayer.Location == locationType);
            }
        }

    }
}
