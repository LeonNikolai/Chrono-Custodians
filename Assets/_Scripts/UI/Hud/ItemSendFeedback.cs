using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
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

    public void SendItem(bool correct, ItemData itemData, TimePeriod targetPeriod, float instabilityModifyValue)
    {
        Debug.Log("ItemSendFeedback Received");
        if (correct)
        {
            if (_itemImage) _itemImage.sprite = itemData.Icon;
            StringVariable itemName = new StringVariable { Value = itemData.Name };
            correctItemTitle.Add("itemName", itemName);
            correctItemExplanation.Add("itemName", itemName);
            StringVariable timePeriodName = new StringVariable { Value = targetPeriod.periodName };
            correctItemExplanation.Add("timePeriodName", timePeriodName);
            StringVariable instabilityChange = new StringVariable { Value = instabilityModifyValue.ToString() };
            correctItemExplanation.Add("instabilityChange", instabilityChange);
            _title.text = correctItemTitle.GetLocalizedString();
            _explanation.text = correctItemExplanation.GetLocalizedString();
            _animator.SetTrigger("Correct");
        }
        else
        {
            if (_itemImage) _itemImage.sprite = itemData.Icon;
            StringVariable itemName = new StringVariable { Value = itemData.Name };
            incorrectItemTitle.Add("itemName", itemName);
            incorrectItemExplanation.Add("itemName", itemName);
            StringVariable timePeriodName = new StringVariable { Value = targetPeriod.periodName };
            incorrectItemExplanation.Add("timePeriodName", timePeriodName);
            StringVariable instabilityChange = new StringVariable { Value = instabilityModifyValue.ToString() };
            incorrectItemExplanation.Add("instabilityChange", instabilityChange);
            _title.text = incorrectItemTitle.GetLocalizedString();
            _explanation.text = incorrectItemExplanation.GetLocalizedString();
            _animator.SetTrigger("Incorrect");
        }
    }
}