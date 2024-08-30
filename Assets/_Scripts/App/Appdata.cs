using System;


// This class is used to store app data and preferences
[Serializable]
public class Appdata
{
    const string DafaultAppdataSaveFileName = "appdata";
    public uint LaunchCount;


    public void Save(string filename = DafaultAppdataSaveFileName)
    {
        SaveSystem.SaveRoot(this, filename);
    }
    public static Appdata Load(string filename = DafaultAppdataSaveFileName)
    {
        return SaveSystem.LoadRoot<Appdata>(filename);
    }
}