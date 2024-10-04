using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Chrono Custodians/LevelScene")]
public class LevelScene : ScriptableObject
{
    [SerializeField, Tooltip("The Scene this level should load")] string _sceneName;
    [Header("Level Info")]
    [SerializeField, Tooltip("The name that should be displayed in the ui")] LocalizedString _levelName;
    [SerializeField] Sprite _previewImage;
    [Header("Time Period")]
    [SerializeField, HideInInspector] int _astronomicalYear;
    public int AstronomicalYear => _astronomicalYear;


    public Sprite PreviewImage => _previewImage;
    public string SceneName => _sceneName;
    public string LevelName => _levelName != null ? _levelName.GetLocalizedString() : _sceneName != "" ? _sceneName : name;


    public void LoadScene()
    {
        LevelManager.LoadLevelScene(this);
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