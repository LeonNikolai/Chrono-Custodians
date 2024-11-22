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
    public LocalizedString wrongItemExplanation;

    public void SendItem(int type, ItemData itemData, TimePeriod targetPeriod)
    {

        StringVariable itemName = new StringVariable { Value = itemData.Name };
        StringVariable timePeriodName = new StringVariable { Value = targetPeriod.periodName };
        switch (type)
        {
            case 0: // Incorrect
                if (_itemImage) _itemImage.sprite = itemData.Icon;
                incorrectItemTitle.Add("itemName", itemName);
                incorrectItemExplanation.Add("itemName", itemName);
                incorrectItemExplanation.Add("timePeriodName", timePeriodName);
                _title.text = incorrectItemTitle.GetLocalizedString();
                _explanation.text = incorrectItemExplanation.GetLocalizedString();
                _animator.SetTrigger("Incorrect");
                break;

            case 1: // Correct
                if (_itemImage) _itemImage.sprite = itemData.Icon;
                correctItemTitle.Add("itemName", itemName);
                correctItemExplanation.Add("itemName", itemName);
                correctItemExplanation.Add("timePeriodName", timePeriodName);
                _title.text = correctItemTitle.GetLocalizedString();
                _explanation.text = correctItemExplanation.GetLocalizedString();
                _animator.SetTrigger("Correct");
                break;

            case 2: // Wrong Item
                if (_itemImage) _itemImage.sprite = itemData.Icon;
                incorrectItemTitle.Add("itemName", itemName);
                wrongItemExplanation.Add("itemName", itemName);
                _title.text = incorrectItemTitle.GetLocalizedString();
                _explanation.text = incorrectItemExplanation.GetLocalizedString();
                _animator.SetTrigger("Incorrect");
                break;
        }

    }
}