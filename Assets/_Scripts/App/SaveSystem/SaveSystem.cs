using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;


public static class SaveSystem
{
    const string JsonFileExtension = ".json";
    const string TextFileExtension = ".txt";
    const string CustomFileExtensions = ".data";
    public const string SlotFolder = "save";

    static string _saveSlot = "default";
    static string _savePath;
    static string _saveSlotFolder;

    public static string SaveRootPath => _savePath;
    public static string SaveSlotFolderPath => _saveSlotFolder;
    public static string CurrentSlotSavePath => $"{SaveSlotFolderPath}/{_saveSlot}/";

    static SaveSystem()
    {
        _savePath = Application.persistentDataPath;
        _saveSlotFolder = _savePath + "/" + SlotFolder;
        Directory.CreateDirectory(Path.GetDirectoryName(SaveSlotFolderPath));
        ActiveSaveSlot = LoadText($"{SaveSlotFolderPath}/currentSaveSlot.txt") ?? "default";
    }

    public static string ActiveSaveSlot
    {
        get => _saveSlot;
        set
        {
            bool isDifferent = _saveSlot != value;
            _saveSlot = value ?? "default";
            if (isDifferent)
            {
                SaveText(_saveSlot, $"{SaveSlotFolderPath}/currentSaveSlot.txt");
            }
            OnSaveSlotChange.Invoke(_saveSlot);
        }
    }

    public static event Action<string> OnSaveSlotChange = delegate { };

    public static string[] GetSaveSlots()
    {
        if (!Directory.Exists(SaveSlotFolderPath)) return new string[0];
        var paths = Directory.GetDirectories(SaveSlotFolderPath);
        for (var i = 0; i < paths.Length; i++) paths[i] = Path.GetFileName(paths[i]);
        return paths;
    }

    // ROOT
    public static T LoadRoot<T>(string filename)
    {
        return LoadJson<T>($"{SaveRootPath}/{filename}{JsonFileExtension}");
    }
    public static string LoadRoot(string filename)
    {
        return LoadText($"{SaveRootPath}/{filename}{TextFileExtension}");
    }
    public static void LoadRoot(string filename, Action<FileStream> readAction)
    {
        LoadSteam($"{SaveRootPath}/{filename}{CustomFileExtensions}", readAction);
    }
    public static void SaveRoot(string data, string filename)
    {
        SaveText(data, $"{SaveRootPath}/{filename}{TextFileExtension}");
    }
    public static void SaveRoot(object data, string filename)
    {
        SaveJson(data, $"{SaveRootPath}/{filename}{JsonFileExtension}");
    }
    public static void SaveRoot(Action<FileStream> writeAction, string filename)
    {
        SaveStream($"{SaveRootPath}/{filename}{CustomFileExtensions}", writeAction);
    }


    // CURRENT SLOT
    public static T LoadCurrentSlot<T>(string filename)
    {
        return LoadSlot<T>(_saveSlot, filename);
    }
    public static string LoadCurrentSlot(string filename)
    {
        return LoadSlot(_saveSlot, filename);
    }
    public static void SaveCurrentSlot(object data, string filename)
    {
        SaveSlot(_saveSlot, data, filename);
    }
    public static void SaveCurrentSlot(string data, string filename)
    {
        SaveSlot(_saveSlot, data, filename);
    }
    public static void SaveCurrentSlot(Action<FileStream> writeAction, string filename)
    {
        SaveSlot(_saveSlot, writeAction, filename);
    }
    public static void LoadCurrentSlot(string filename, Action<FileStream> readAction)
    {
        LoadSlot(_saveSlot, filename, readAction);
    }

    // SLOT
    public static T LoadSlot<T>(string slot, string filename)
    {
        return LoadJson<T>($"{SaveSlotFolderPath}/{slot}/{filename}{JsonFileExtension}");
    }
    public static string LoadSlot(string slot, string filename)
    {
   
        return LoadText($"{SaveSlotFolderPath}/{slot}/{filename}{TextFileExtension}");
    }
    public static void LoadSlot(string slot, string filename, Action<FileStream> readAction)
    {
        LoadSteam($"{SaveSlotFolderPath}/{slot}/{filename}{CustomFileExtensions}", readAction);
    }

    public static void SaveSlot(string slot, object data, string filename)
    {
        SaveJson(data, $"{SaveSlotFolderPath}/{slot}/{filename}{JsonFileExtension}");
    }
    public static void SaveSlot(string slot, string data, string filename)
    {
        SaveText(data, $"{SaveSlotFolderPath}/{slot}/{filename}{TextFileExtension}");
    }
    public static void SaveSlot(string slot, Action<FileStream> writeAction, string filename)
    {
        SaveStream($"{SaveSlotFolderPath}/{slot}/{filename}{CustomFileExtensions}", writeAction);
    }


    // Monobehaviour Save
    public static void SaveCurrentSlotMono(this MonoBehaviour data, string path)
    {
        SaveSlotMono(data, ActiveSaveSlot, path);
    }
    public static void LoadCurrentSlotMono(this MonoBehaviour target, string path)
    {
        LoadSlotMono(target, ActiveSaveSlot, path);
    }
    public static void SaveSlotMono(this MonoBehaviour data, string slot, string path)
    {
        path = $"{SaveSlotFolderPath}/{slot}/{path}{JsonFileExtension}";
        var json = JsonUtility.ToJson((object)data, true);
        SaveText(json, path);
    }
    public static void LoadSlotMono(this MonoBehaviour target, string slot, string path)
    {
        path = $"{SaveSlotFolderPath}/{slot}/{path}{JsonFileExtension}";
        string data = LoadText(path);
        OverrideFromJson(target, data);
    }

    // GENERIC
    public static void OverrideFromJson(object obj, string data)
    {
        JsonUtility.FromJsonOverwrite(data, obj);
    }
    static void SaveJson(object data, string fullpath)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        SaveText(json, fullpath);
    }
    static T LoadJson<T>(string path)
    {
        string data = LoadText(path);
        if (data is null) return default;
        T dataObject = JsonConvert.DeserializeObject<T>(data);
        return dataObject ?? default;
    }

    static void SaveText(string text, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, text);
    }
    static string LoadText(string path)
    {
        if (File.Exists(path)) return File.ReadAllText(path);
        return null;
    }

    static void SaveStream(string path, Action<FileStream> streamFunc)
    {
        var stream = new FileStream(path, FileMode.Create);
        streamFunc(stream);
        stream.Close();
    }

    static bool LoadSteam(string path, Action<FileStream> streamFunc)
    {

        if (File.Exists(path))
        {
            var stream = new FileStream(path, FileMode.Open);
            if (stream.Length == 0)
            {
                stream.Close();
                return false;
            }
            streamFunc(stream);
            stream.Close();
            return true;
        }
        return false;
    }

    static void SaveImage(Texture2D texture, string path)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }
    static Texture2D LoadImage(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        return texture;
    }


    public static void DeleteGameSlots()
    {
        Directory.Delete(SaveSlotFolderPath + "/", true);
        Directory.CreateDirectory(Path.GetDirectoryName(SaveSlotFolderPath + "/"));
    }
    public static void DeleteEverything()
    {
        Directory.Delete(SaveRootPath + "/", true);
        Directory.CreateDirectory(Path.GetDirectoryName(SaveRootPath + "/"));
    }
    public static void DeleteGameSlot(string slot)
    {
        if (Directory.Exists($"{SaveSlotFolderPath}/{slot}"))
        {
            Directory.Delete($"{SaveSlotFolderPath}/{slot}", true);
        }
    }
}