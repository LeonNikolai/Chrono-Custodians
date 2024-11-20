using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
# endif
[CreateAssetMenu(menuName = "Chrono Custodians/ItemIdProvider")]
public class ItemIdProvider : ScriptableObject
{
    [SerializeField] private ItemData[] _itemData;
    public ItemData[] ItemDatas => _itemData;
    [SerializeField] private TimePeriod[] _timePeriod;
    [SerializeField] private LocationRenderingSettings[] _locationRenderingSettings;
    [SerializeField] private LevelScene[] levelScenes;
    public TimePeriod[] TimePeriods => _timePeriod;
    public LocationRenderingSettings[] LocationRenderingSettings => _locationRenderingSettings;
    public LevelScene[] Levels => levelScenes;

    [ContextMenu("Find All Items")]
    public void FindAllItems()
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets" });
        _itemData = new ItemData[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            _itemData[i] = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
        }

        guids = AssetDatabase.FindAssets("t:TimePeriod", new[] { "Assets" });
        _timePeriod = new TimePeriod[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            _timePeriod[i] = AssetDatabase.LoadAssetAtPath<TimePeriod>(assetPath);
        }

        guids = AssetDatabase.FindAssets("t:LocationRenderingSettings", new[] { "Assets" });
        _locationRenderingSettings = new LocationRenderingSettings[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            _locationRenderingSettings[i] = AssetDatabase.LoadAssetAtPath<LocationRenderingSettings>(assetPath);
        }

        guids = AssetDatabase.FindAssets("t:LevelScene", new[] { "Assets" });
        levelScenes = new LevelScene[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            levelScenes[i] = AssetDatabase.LoadAssetAtPath<LevelScene>(assetPath);
        }
#endif
    }

    public int GetItemId(ItemData itemData)
    {
        for (int i = 0; i < _itemData.Length; i++)
        {
            if (_itemData[i] == itemData)
            {
                return i;
            }
        }
        Debug.LogError("ItemData not found in ItemIdProvider");
        return -1;
    }
    public int GetTimeId(TimePeriod timePeriod)
    {
        for (int i = 0; i < _timePeriod.Length; i++)
        {
            if (_timePeriod[i] == timePeriod)
            {
                return i;
            }
        }
        Debug.LogError("timePeriod not found in ItemIdProvider");
        return -1;
    }
    public int GetRenderingSettingsId(LocationRenderingSettings locationRenderingSettings)
    {
        for (int i = 0; i < this._locationRenderingSettings.Length; i++)
        {
            if (this._locationRenderingSettings[i] == locationRenderingSettings)
            {
                return i;
            }
        }
        Debug.LogError("locationRenderingSettings not found in ItemIdProvider");
        return -1;
    }

    public int GetLevelSceneId(LevelScene levelScene)
    {
        for (int i = 0; i < levelScenes.Length; i++)
        {
            if (levelScenes[i] == levelScene)
            {
                return i;
            }
        }
        Debug.LogError("LevelScene not found in ItemIdProvider");
        return -1;
    }
    public ItemData GetItemData(int id)
    {
        if (id < 0 || id >= _itemData.Length)
        {
            Debug.LogError("ItemData not found in ItemIdProvider");
        }
        return _itemData[id];
    }
    public TimePeriod GetPeriodData(int id)
    {
        if (id < 0 || id >= _timePeriod.Length)
        {
            Debug.LogError($"TimePeriod with ID: {id} not found in ItemIdProvider");
        }
        return _timePeriod[id];
    }
    public LocationRenderingSettings GetLocationRenderingSettings(int id)
    {
        if (id < 0 || id >= _locationRenderingSettings.Length)
        {
            Debug.LogError($"LocationRenderingSettings with ID: {id} not found in ItemIdProvider");
        }
        return _locationRenderingSettings[id];
    }

    public LevelScene GetLevelScene(int id)
    {
        if (id < 0 || id >= levelScenes.Length)
        {
            Debug.LogError($"LevelScene at index {id} not found in ItemIdProvider (Length: {levelScenes.Length})");
        }
        return levelScenes[id];
    }


}