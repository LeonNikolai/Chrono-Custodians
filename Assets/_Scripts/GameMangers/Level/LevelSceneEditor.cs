#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Editor

[CustomEditor(typeof(LevelScene))]
public class LevelSceneEditor : Editor
{
    private LevelScene levelScene;

    private void OnEnable()
    {
        levelScene = (LevelScene)target;
    }

    public override void OnInspectorGUI()
    {
        var astr = levelScene.AstronomicalYear;
        var ISOText = levelScene.ISOYear(astr);
        var Greg = levelScene.GregorianYear(astr);
        var Common = levelScene.CommonYear(astr);
        var HumanCommon = levelScene.HumanEra(astr);
        DrawDefaultInspector();


        var AstronomicalYear = serializedObject.FindProperty("_astronomicalYear");
        EditorGUILayout.PropertyField(AstronomicalYear);
        serializedObject.ApplyModifiedProperties();


        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.richText = true;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"<color=grey>Common Era :</color>", style);
        EditorGUILayout.LabelField($"{Common}", style);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"<color=grey>Gregorian : </color>", style);
        EditorGUILayout.LabelField($"{Greg}", style);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"<color=grey>ISO 8601 : </color>", style);
        EditorGUILayout.LabelField($"{ISOText}", style);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"<color=grey>Human Era: </color>", style);
        EditorGUILayout.LabelField($"{HumanCommon}", style);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"<color=grey>Galactical Year: </color>", style);
        EditorGUILayout.LabelField($"{((float)astr / 225_000_000.0).ToString("0.000000")} gal", style);
        EditorGUILayout.EndHorizontal();
    }
}

#endif