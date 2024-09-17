using System;
using System.ComponentModel;
using UnityEngine;
/// <summary>
/// This class is responsible for the HUD of the game.
/// </summary>
public class Hud : MonoBehaviour
{
    [Header("References")]
    [SerializeField] UiBar _healthBar;
    [SerializeField] UiBar _staminaBar;
    [SerializeField] UiBar _shieldBar;
    [SerializeField] CanvasGroup _hudRoot;
    [SerializeField] HudItemIcon[] _inventoryIcons;

    public static float Opacity
    {
        set => instance._hudRoot.alpha = value;
    }
    public static float Health
    {
        set => instance._healthBar.Progress = value;
    }

    public static float Stamina
    {
        set => instance._staminaBar.Progress = value;
    }

    public static void SetInventoryIcon(ItemData itemdata, int index, bool selected)
    {
        if(index < instance._inventoryIcons.Length)
        {
            instance._inventoryIcons[index].SetIcon(itemdata, selected);
            return;
        }
    }

    public static Hud instance = null;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}