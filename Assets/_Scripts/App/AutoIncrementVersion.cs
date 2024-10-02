#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
public class AutoIncrementVersion : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        string currentVersion = PlayerSettings.bundleVersion;

        // Major.Minor.Patch.Build
        string[] versionParts1 = currentVersion.Split('-');
        string[] versionParts = versionParts1[0].Split('.');

        if (versionParts.Length < 4)
        {
            versionParts = new string[4];
            versionParts[0] = "0";
            versionParts[1] = "0";
            versionParts[2] = "0";
            versionParts[3] = "0";
        }

        int buildNumber;
        if (int.TryParse(versionParts[3], out buildNumber))
        {
            buildNumber++;
            versionParts[3] = buildNumber.ToString();
        }
        string newVersion = string.Join(".", versionParts);
        PlayerSettings.bundleVersion = newVersion;
        PlayerSettings.bundleVersion += " - " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Debug.Log("Auto-Incremented Version: " + newVersion);
        PlayerSettings.iOS.buildNumber = buildNumber.ToString();
        PlayerSettings.Android.bundleVersionCode = buildNumber;
        AssetDatabase.SaveAssets();
    }
}

#endif