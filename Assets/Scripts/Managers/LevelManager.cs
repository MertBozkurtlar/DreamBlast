using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    [Header("Level Configuration")]
    [SerializeField] private PlayerProgressSO playerProgress;
    [SerializeField] private string gameSceneName = "GameScene";
    public PlayerProgressSO PlayerProgress => playerProgress;
    public TextAsset[] LevelJsonFiles => playerProgress.LevelJsonFiles;

    protected override void Init()
    {
        base.Init();
        if (LevelJsonFiles == null || LevelJsonFiles.Length == 0)
        {
            Debug.LogError("No level JSON files assigned to LevelManager! Please assign them in the Unity Inspector.");
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void StartLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < LevelJsonFiles.Length)
        {
            playerProgress.SetCurrentLevel(levelIndex);
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError($"Level index {levelIndex} is invalid!");
        }
    }

    public void StartNextLevel()
    {
        int nextLevel = playerProgress.CurrentLevel + 1;
        StartLevel(nextLevel);
    }

    public void StartCurrentLevel()
    {
        StartLevel(playerProgress.CurrentLevel);
    }

    public void UnlockNextLevel()
    {
        int nextLevel = playerProgress.CurrentLevel + 1;
        playerProgress.SetCurrentLevel(nextLevel);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
} 