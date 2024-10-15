using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class ItemSendFeedback : MonoBehaviour
{
    [Header("References")]
    public TMP_Text _title;
    public TMP_Text _explanation;
    public Image _itemImage;
    public Animator _animator;

    [Header("Feedback")]
    public LocalizedString correctItemTitle;
    public LocalizedString incorrectItemTitle;
    public LocalizedString correctItemExplanation;
    public LocalizedString incorrectItemExplanation;

    public void SendItem(bool correct, ItemData itemData, TimePeriod targetPeriod, float instabilityChange)
    {
        if (correct)
        {
            if (_itemImage) _itemImage.sprite = itemData.Icon;
            var itemName = itemData.Name;
            _title.text = correctItemTitle.GetLocalizedString(itemName);
            _explanation.text = correctItemExplanation.GetLocalizedString(targetPeriod.periodName, instabilityChange.ToString());
            _animator.SetTrigger("Correct");
        }
        else
        {
            if (_itemImage) _itemImage.sprite = itemData.Icon;
            var itemName = itemData.Name;
            _title.text = incorrectItemTitle.GetLocalizedString(itemName);
            _explanation.text = incorrectItemExplanation.GetLocalizedString(targetPeriod.periodName, instabilityChange.ToString());
            _animator.SetTrigger("Incorrect");
        }
    }
}