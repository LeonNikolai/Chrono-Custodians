

using JetBrains.Annotations;
using Unity.Collections;

public class CustomUserData {
    const string FileName = "playerData";
    public string PlayerName = "Username";

    public void Save() {
        // Save the player data
        SaveSystem.SaveRoot(this,FileName);
    }

    public void Load() {
        var data = SaveSystem.LoadRoot<CustomUserData>(FileName) ?? new CustomUserData();
        PlayerName = data.PlayerName;
    }
}