/// <summary>
/// A Script that is meant to initalize services and stuff on startup.
/// </summary>
public static class App
{
    public static bool IsDebug = true;
    public static bool IsRunning { get; private set; }
    public static Appdata appdata = new Appdata();
    public static uint LaunchCount
    {
        get => appdata.LaunchCount;
        set
        {
            appdata.LaunchCount = value;
            appdata.Save();
        }
    }
    internal static void OnApplicationAwake()
    {
        // First thing to do when the application starts 
        // before the splash screen
        LoadAppdata();
        LaunchCount++;
        IsRunning = true;
    }

    public static void OnApplicationStart()
    {
        // After the first scene is loaded

    }
    internal static void OnFrame()
    {
        // Before every frame

    }
    internal static void OnApplicationEnd()
    {
        // When the application is about to close
        SaveAppdata();
        IsRunning = false;
    }

    public static void SaveAppdata()
    {
        appdata?.Save();
    }
    public static void LoadAppdata()
    {
        appdata = Appdata.Load() ?? new Appdata();
    }
    public static void ClearData()
    {
        SaveSystem.DeleteGameSlots();
        appdata = new Appdata();
        appdata?.Save();
    }
}
