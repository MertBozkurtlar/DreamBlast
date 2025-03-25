using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProgress", menuName = "Player Progress")]
public class PlayerProgressSO : ScriptableObject
{
    [SerializeField] private TextAsset[] levelJsonFiles;
    [SerializeField] private int currentLevel;

    private const string CURRENT_LEVEL_KEY = "CurrentLevel";

    public int CurrentLevel => currentLevel;
    public TextAsset[] LevelJsonFiles => levelJsonFiles;

    private void OnEnable()
    {
        LoadProgress();
    }

    public void SetCurrentLevel(int level)
    {
        currentLevel = Mathf.Max(0, level);
        SaveProgress();
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, currentLevel);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        currentLevel = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0);
    }
} 