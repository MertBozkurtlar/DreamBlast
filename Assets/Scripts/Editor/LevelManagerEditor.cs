#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    private int levelToLoad = 0;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        LevelManager levelManager = (LevelManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Controls", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Restart Level"))
        {
            levelManager.RestartLevel();
        }
        
        if (GUILayout.Button("Load Next Level"))
        {
            levelManager.LoadNextLevel();
        }
        
        EditorGUILayout.Space();
        
        levelToLoad = EditorGUILayout.IntField("Level Index", levelToLoad);
        if (GUILayout.Button("Load Specific Level"))
        {
            levelManager.LoadLevel(levelToLoad);
        }
    }
}
#endif 