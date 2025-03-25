#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    private int levelToLoad = 0;
    private int levelToSet = 0;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        LevelManager levelManager = (LevelManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Controls", EditorStyles.boldLabel);
        levelToSet = EditorGUILayout.IntField("Level Index", levelToSet);
        if (GUILayout.Button("Set Current Level"))
        {
            levelManager.PlayerProgress.SetCurrentLevel(levelToSet);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Load Scene", EditorStyles.boldLabel);
        levelToLoad = EditorGUILayout.IntField("Level Index", levelToLoad);
        if (GUILayout.Button("Start Level"))
        {
            levelManager.StartLevel(levelToLoad);
        }

        if (GUILayout.Button("Load Main Menu"))
        {
            levelManager.LoadMainMenu();
        }
    }
}
#endif 