
[System.Serializable]
public class GameSaveData
{
    const string FileName = "SaveData";
    public float Instability;

    public static void Load(Game game)
    {
        var data = SaveSystem.LoadCurrentSlot<GameSaveData>("SaveData");
    }
    public static void Save(Game game)
    {
        GameSaveData saveData = new GameSaveData();
        SaveSystem.SaveCurrentSlot(saveData, "SaveData");
    }
}