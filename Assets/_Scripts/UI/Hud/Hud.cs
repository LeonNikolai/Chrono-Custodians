using TMPro;
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
    [SerializeField] TMP_Text _crosshairTooltip;
    [SerializeField] TMP_Text _itemTooltip;
    [SerializeField] TMP_Text shortScannerNotificationText;
    [SerializeField] GameObject shortScannerNotification;
    [SerializeField] CanvasGroup _hudRoot;
    [SerializeField] HudItemIcon[] _inventoryIcons;
    [SerializeField] ItemSendFeedback _itemSendFeedback;


    public static bool Hidden
    {
        get => !instance._hudRoot.gameObject.activeSelf;
        set => instance._hudRoot.gameObject.SetActive(!value);
    }
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
    public static string CrosshairTooltip
    {
        set
        {
            instance._crosshairTooltip.text = value;
            if (value == "") instance._crosshairTooltip.gameObject.SetActive(false);
            else instance._crosshairTooltip.gameObject.SetActive(true);
        }
        get
        {
            return instance?._crosshairTooltip?.text ?? "";
        }
    }
    public static string ItemTooltip
    {
        set
        {
            instance._itemTooltip.text = value;
        }
        get
        {
            return instance?._itemTooltip?.text ?? "";
        }
    }
    public static string ScannerNotification
    {
        set
        {
            if(instance == null) return;
            if(instance.shortScannerNotificationText == null) return;
            if(instance.shortScannerNotification == null) return;
            if (string.IsNullOrEmpty(value))
            {
                instance.shortScannerNotification.SetActive(false);
                instance.shortScannerNotificationText.text = value;
                return;
            }
            instance.shortScannerNotificationText.text = value;
            instance.shortScannerNotification.SetActive(true);
        }
        get
        {
            return instance?.shortScannerNotificationText?.text ?? "";
        }
    }



    public static void SetInventoryIcon(ItemData itemdata, int index, bool selected)
    {
        if (index < instance._inventoryIcons.Length)
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

    internal static void ItemSentFeedback(int type, ItemData itemData, TimePeriod targetPeriod, float instabilityChange)
    {
        Debug.Log("Hud Received");
        instance._itemSendFeedback.SendItem(type, itemData, targetPeriod, instabilityChange);
    }
}