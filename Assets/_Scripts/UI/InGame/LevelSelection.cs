using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{
    [Header("Logic References")]
    [SerializeField] private LevelScene _levelScene;
    [SerializeField] private Button _button;
    [Header("Visual References")]
    [SerializeField] private TMP_Text _Name;
    [SerializeField] private TMP_Text _Year;
    public void LoadLevel()
    {
        _levelScene.LoadRandomScene();
    }

    void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (_button) _button.onClick.AddListener(LoadLevel);

        }
        if (_Name) _Name.text = _levelScene.LevelName;
        if (_Year) _Year.text = TimeConverter.ToCommonYear(_levelScene.AstronomicalYear);
    }

    public void SetLevelForSelection(LevelScene levelScene)
    {
        _levelScene = levelScene;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_Name) _Name.text = _levelScene.LevelName;
        if (_Year) _Year.text = TimeConverter.ToCommonYear(_levelScene.AstronomicalYear);
    }
}
