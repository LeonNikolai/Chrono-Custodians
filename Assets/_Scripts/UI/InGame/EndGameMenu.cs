using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class EndGameMenu : MonoBehaviour
{
    public TMP_Text _resultTextDisplay;
    public TMP_Text _resultReasonDisplay;
    public Button _restartButton;

    public LocalizedString _winText = null;
    public LocalizedString _failText = null;
    public LocalizedString _allPlayersDied = null;
    public LocalizedString _instabilityText = null;

    void Start()
    {
        if(_restartButton) _restartButton.onClick.AddListener(Reload);
    }
    void OnEnable()
    {
        if(GameManager.instance) GameManager.instance.levelState.OnValueChanged += UpdateData;
        UpdateVisuals();
    }

    private void UpdateData(GameManager.LevelState previousValue, GameManager.LevelState newValue)
    {
        UpdateVisuals();
    }

    void OnDisable()
    {
        if(GameManager.instance) GameManager.instance.levelState.OnValueChanged -= UpdateData;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if(!GameManager.instance) return;
        if (GameManager.instance.levelState.Value == GameManager.LevelState.End_Lose)
        {
            _resultTextDisplay.text = _failText != null ? _failText.GetLocalizedString() : "You have failed";
            if (Player.AllPlayersAreDead)
            {
                _resultReasonDisplay.text = _allPlayersDied != null ? _allPlayersDied.GetLocalizedString() : "All players have died";
            }
            else
            {
                _resultReasonDisplay.text = _instabilityText != null ? _instabilityText.GetLocalizedString() : "Timebubble de-stabilized";
            }
        }
        if (GameManager.instance.levelState.Value == GameManager.LevelState.End_Win)
        {

            _resultTextDisplay.text = _winText != null ? _winText.GetLocalizedString() : "You have won";
            _resultReasonDisplay.text = "";
        }
    }

    private void Reload()
    {
        LevelManager.instance.Reload();
    }

    void OnDestroy()
    {
    }



}
