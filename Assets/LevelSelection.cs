using UnityEngine;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private LevelScene _levelScene;
    public void LoadLevel() =>_levelScene.LoadScene();
    void Start()
    {
        if(_button) _button.onClick.AddListener(LoadLevel);
    }
}
