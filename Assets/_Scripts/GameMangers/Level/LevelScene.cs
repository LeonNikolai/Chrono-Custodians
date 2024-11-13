using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Chrono Custodians/LevelScene")]
public class LevelScene : ScriptableObject
{
    [Header("Scene Info (Random Scene picked if multiple)")]
    [SerializeField, Tooltip("The Scene this level should load")] string[] _sceneNames;
    [Header("Level Info")]
    [SerializeField, Tooltip("The name that should be displayed in the ui")] LocalizedString _levelName;
    [SerializeField] Sprite _previewImage;
    [Header("Time Period")]
    [SerializeField, HideInInspector] int _astronomicalYear;
    [SerializeField] TimePeriod _timePeriod;
    public int AstronomicalYear => _astronomicalYear;
    public TimePeriod TimePeriod => _timePeriod;


    public Sprite PreviewImage => _previewImage;
    public string FirstSceneName => _sceneNames != null && _sceneNames.Length > 0 ? _sceneNames[0] : "";
    public string SceneNameFromIndex(int index)
    {
        if (index == -1) return RandomSceneName;
        return _sceneNames != null && _sceneNames.Length > 0 ? _sceneNames[index % _sceneNames.Length] : "";
    }
    public string RandomSceneName => _sceneNames != null && _sceneNames.Length > 0 ? _sceneNames[UnityEngine.Random.Range(0, _sceneNames.Length)] : "";
    public string LevelName => _levelName != null && !_levelName.IsEmpty ? _levelName.GetLocalizedString() : FirstSceneName != "" ? FirstSceneName : name;


    public void LoadScene(int index = 0)
    {
        if (index == -1)
        {
            LoadRandomScene();
            return;
        }
        LevelManager.LoadLevelScene(this, index);
    }
    public void LoadRandomScene()
    {
        LevelManager.LoadLevelScene(this);
    }
    public NetworkLevel GetNetworklevel()
    {
        return new NetworkLevel(this);
    }
    public LevelStability GetStability()
    {
        return GameManager.instance.GetLevelStability(this);
    }
}

public static class TimeConverter
{
    public static string ToISOYear(int astronomicalYear)
    {
        return astronomicalYear.ToString("D4");
    }
    public static string ToGregorianYear(int astronomicalYear)
    {
        if (astronomicalYear <= 0)
        {
            // 1 BC is year 0 so we need to add 1 to convert 0 to 1
            return $"{-astronomicalYear + 1} BC";
        }
        else
        {
            return $"{astronomicalYear} AD";
        }
    }
    public static string ToCommonYear(int astronomicalYear)
    {
        if (astronomicalYear <= 0)
        {
            // 1 BC is year 0 so we need to add 1 to convert 0 to 1
            return $"{-astronomicalYear + 1} BCE";
        }
        else
        {
            return $"{astronomicalYear} CE";
        }
    }
    public static string ToHumanEraYear(int astronomicalYear)
    {
        return $"{astronomicalYear + 10000} HE";
    }


}