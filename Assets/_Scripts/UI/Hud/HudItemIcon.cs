
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudItemIcon : MonoBehaviour
{
    [SerializeField] Image _icon;
    [SerializeField] Image _selectionFrame;
    [SerializeField] TMP_Text _ItemName;
    void Awake()
    {
        _selectionFrame.enabled = false;
        _icon.sprite = null;
        _icon.enabled = false;
        if (_ItemName) _ItemName.text = "";
    }
    public void SetIcon(ItemData icon, bool isEquipped)
    {

        _selectionFrame.enabled = isEquipped;

        if (icon == null)
        {
            if (_ItemName) _ItemName.text = "";
            _icon.sprite = null;
            _icon.enabled = false;
            return;
        }
        if (_ItemName) _ItemName.text = icon.Name;
        _icon.sprite = icon.Icon;
        _icon.enabled = true;
    }
}