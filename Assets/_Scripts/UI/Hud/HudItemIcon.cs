
using UnityEngine;
using UnityEngine.UI;

public class HudItemIcon : MonoBehaviour
{
    [SerializeField] Image _icon;
    [SerializeField] Image _selectionFrame;
    void Awake()
    {
        _selectionFrame.enabled = false;
        _icon.sprite = null;
        _icon.enabled = false;
    }
    public void SetIcon(ItemData icon, bool isEquipped)
    {

        _selectionFrame.enabled = isEquipped;

        if (icon == null)
        {
            _icon.sprite = null;
            _icon.enabled = false;
            return;
        }

        _icon.sprite = icon.Icon;
        _icon.enabled = true;
    }
}