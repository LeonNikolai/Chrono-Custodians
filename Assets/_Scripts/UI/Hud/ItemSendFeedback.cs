using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class ItemSendFeedback : MonoBehaviour
{
    [Header("References")]
    public TMP_Text _tittle;
    public TMP_Text _explenation;
    public Image _itemImage;
    public Animator _animator;

    [Header("Feedback")]
    public LocalizedString correctItemTittle;
    public LocalizedString incorrectItemTittle;
    public LocalizedString correctItemExplenation;
    public LocalizedString incorrectItemExplenation;

    public void SendItem(bool correct, ItemData itemData, int targetYear, float instabilityChange)
    {
        if (correct)
        {
            if (_itemImage) _itemImage.sprite = itemData.Icon;
            var itemName = itemData.Name;
            _tittle.text = correctItemTittle.GetLocalizedString(itemName);
            _explenation.text = correctItemExplenation.GetLocalizedString(targetYear.ToString(), instabilityChange.ToString());
            _animator.SetTrigger("Correct");
        }
        else
        {
            if (_itemImage) _itemImage.sprite = itemData.Icon;
            var itemName = itemData.Name;
            _tittle.text = incorrectItemTittle.GetLocalizedString(itemName);
            _explenation.text = incorrectItemExplenation.GetLocalizedString(targetYear.ToString(), instabilityChange.ToString());
            _animator.SetTrigger("Incorrect");
        }
    }
}