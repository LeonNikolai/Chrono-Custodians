using UnityEngine;

[CreateAssetMenu(menuName = "Chrono Custodians/LevelScene")]
public class LevelScene : ScriptableObject
{
    [SerializeField] string _sceneName;
    [SerializeField] string _levelName;
    [SerializeField] Sprite _previewImage;
    public Sprite PreviewImage => _previewImage;
    public string SceneName => _sceneName;
    public string LevelName => _levelName;


    public void LoadScene()
    {
        LevelManager.LoadLevelScene(this);
    }
}