using UnityEngine;
using System.Collections;
using DG.Tweening;
using DreamBlast.Data;
using System;
using Unity.VisualScripting;

public class GameManager : Singleton<GameManager>
{
    #region Fields
    [SerializeField] private PlayerProgressSO playerProgress;

    [Header("UI References")]
    [SerializeField] private GameUIController gameUIController;
    [SerializeField] private ParticleSystem celebrationParticles;
    [SerializeField] private GameObject gameOverUI;

    private GameGrid gameGrid;
    private GridItemPool itemPool;
    private int remainingMoves;
    private int remainingObstacles;
    private System.Collections.Generic.Dictionary<string, DreamBlast.Data.ObstacleCount> obstacleCounters = new System.Collections.Generic.Dictionary<string, DreamBlast.Data.ObstacleCount>();
    #endregion

    #region State Management
    public enum GameState
    {
        Playing,
        LevelCompleted,
        GameOver,
        Loading
    }
    private GameState currentState = GameState.Playing;
    public GameState CurrentState => currentState;
    #endregion


    protected override void Init()
    {
        base.Init();
        
        if (playerProgress == null)
        {
            Debug.LogError("PlayerProgressSO is not assigned to GameManager! Please assign it in the Unity Inspector.");
            return;
        }
        InitializeObstacleCounters();
    }

    private void Start()
    {
        StartCoroutine(WaitForDependencies());
    }

    private IEnumerator WaitForDependencies()
    {
        while (GameGrid.Instance == null)
        {
            yield return null;
        }
        gameGrid = GameGrid.Instance as GameGrid;

        while (GridItemPool.Instance == null)
        {
            yield return null;
        }
        itemPool = GridItemPool.Instance as GridItemPool;

        while (LevelManager.Instance == null){
            yield return null;
        }

        ConnectGridEvents();
        LoadCurrentLevel();
    }

    private void LoadCurrentLevel()
    {
        int currentLevel = playerProgress.CurrentLevel;
        if (currentLevel < 0 || currentLevel >= LevelManager.Instance.LevelJsonFiles.Length)
        {
            Debug.LogError($"Invalid current level {currentLevel}! Level index must be between 0 and {LevelManager.Instance.LevelJsonFiles.Length - 1}.");
            return;
        }

        TextAsset levelJson = LevelManager.Instance.LevelJsonFiles[currentLevel];
        LoadLevelFromJson(levelJson);
    }

    private void LoadLevelFromJson(TextAsset jsonFile)
    {
        if (!ValidateLevelData(jsonFile)) return;

        try
        {
            LevelData levelData = ParseLevelData(jsonFile);
            ApplyLevelData(levelData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading level: {e.Message}");
        }
    }

    private bool ValidateLevelData(TextAsset jsonFile)
    {
        if (jsonFile == null)
        {
            Debug.LogError("Level JSON file is null!");
            return false;
        }
        return true;
    }

    private LevelData ParseLevelData(TextAsset jsonFile)
    {
        return JsonUtility.FromJson<LevelData>(jsonFile.text);
    }

    private void ApplyLevelData(LevelData levelData)
    {
        // Initialize grid with new dimensions
        Vector2Int gridDimensions = new Vector2Int(levelData.grid_width, levelData.grid_height);
        gameGrid.InitializeGrid(gridDimensions);

        // Reset game state
        ResetObstacleCounts();

        // Place items on grid
        PlaceGridItems(levelData);

        // Initialize game state with move count
        InitializeGameState(levelData.move_count);
    }

    private void PlaceGridItems(LevelData levelData)
    {
        if (levelData.grid.Length != levelData.grid_width * levelData.grid_height)
        {
            Debug.LogError("Grid data length doesn't match grid dimensions!");
            return;
        }

        for (int i = 0; i < levelData.grid.Length; i++)
        {
            int x = i % levelData.grid_width;
            int y = i / levelData.grid_width;
            string itemCode = levelData.grid[i];

            if (string.IsNullOrEmpty(itemCode)) continue;

            GridItem item = CreateItemFromCode(itemCode);
            if (item != null)
            {
                gameGrid.PlaceItemOnGrid(item, x, y);
            }
        }

        gameGrid.UpdateAllGroups();
    }

    private GridItem CreateItemFromCode(string itemCode)
    {
        GridItem item = null;

        if (IsColorCube(itemCode))
        {
            item = itemPool.GetCubeOfType(itemCode);
        }
        else if (IsPowerUp(itemCode))
        {
            item = itemPool.GetPowerUp(itemCode);
        }
        else if (IsObstacle(itemCode))
        {
            item = itemPool.GetObstacle(itemCode);
            UpdateObstacleCounts(itemCode);
        }

        return item;
    }

    private bool IsColorCube(string code)
    {
        return code == "r" || code == "g" || code == "b" || code == "y" || code == "rand";
    }

    private bool IsPowerUp(string code)
    {
        return code == "hro" || code == "vro";
    }

    private bool IsObstacle(string code)
    {
        return code == "bo" || code == "s" || code == "v";
    }

    private void InitializeObstacleCounters()
    {
        obstacleCounters.Add("bo", new DreamBlast.Data.ObstacleCount("Box"));
        obstacleCounters.Add("s", new DreamBlast.Data.ObstacleCount("Stone"));
        obstacleCounters.Add("v", new DreamBlast.Data.ObstacleCount("Vase"));
    }

    public void InitializeGameState(int moveCount)
    {
        currentState = GameState.Playing;
        remainingMoves = moveCount;
        gameOverUI.SetActive(false);
        UpdateUI();
    }

    private void ConnectGridEvents()
    {
        if (gameGrid != null)
        {
            gameGrid.onObstacleDestroyed += OnObstacleDestroyed;
            gameGrid.onMoveMade += OnMoveMade;
        }
    }

    protected override void Destroy()
    {
        if (gameGrid != null)
        {
            gameGrid.onObstacleDestroyed -= OnObstacleDestroyed;
            gameGrid.onMoveMade -= OnMoveMade;
        }
    }

    public void OnObstacleDestroyed(string obstacleType)
    {
        if (obstacleCounters.TryGetValue(obstacleType, out DreamBlast.Data.ObstacleCount counter))
        {
            counter.Decrease();
            remainingObstacles--;
            CheckGameState();
            UpdateUI();
        }
    }

    public void OnMoveMade()
    {
        remainingMoves--;
        UpdateUI();
        CheckGameState();
    }

    private void UpdateUI()
    {
        gameUIController.UpdateMoveCountUI(remainingMoves);
        gameUIController.UpdateGoalUI(obstacleCounters);
    }

    private void CheckGameState()
    {
        if (currentState != GameState.Playing)
            return;

        if (DOTween.PlayingTweens()?.Count > 0 || gameGrid.IsProcessingMatches)
        {
            StartCoroutine(WaitAndCheckGameState());
            return;
        }

        if (remainingObstacles <= 0)
        {
            currentState = GameState.LevelCompleted;
            celebrationParticles.Play();
            LevelManager.Instance.UnlockNextLevel();
            StartCoroutine(WaitForAnimationsToComplete(() => LevelManager.Instance.LoadMainMenu()));
        }
        else if (remainingMoves <= 0)
        {
            currentState = GameState.GameOver;
            StartCoroutine(WaitForAnimationsToComplete(() => {
                gameOverUI.transform.localScale = Vector3.zero;
                gameOverUI.SetActive(true);
                gameOverUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            }));
        }
    }

    private IEnumerator WaitAndCheckGameState()
    {
        while (DOTween.PlayingTweens()?.Count > 0 || gameGrid.IsProcessingMatches)
        {
            yield return null;
        }
        CheckGameState();
    }

    private IEnumerator WaitForAnimationsToComplete(System.Action callback)
    {
        while (DOTween.PlayingTweens()?.Count > 0 || celebrationParticles.isPlaying)
        {
            yield return null;
        }
        callback?.Invoke();
    }

    public void RestartLevel()
    {
        gameOverUI.SetActive(false);
        LevelManager.Instance.StartCurrentLevel();
    }

    public void LoadNextLevel()
    {
        LevelManager.Instance.StartNextLevel();
    }

    public void UpdateObstacleCounts(string obstacleType)
    {
        if (obstacleCounters.TryGetValue(obstacleType, out DreamBlast.Data.ObstacleCount counter))
        {
            counter.Increase();
            remainingObstacles++;
        }
    }

    public void ResetObstacleCounts()
    {
        foreach (var counter in obstacleCounters.Values)
        {
            counter.Reset();
        }
        remainingObstacles = 0;
    }

    [System.Serializable]
    private class LevelData
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;
        public string[] grid;
    }
} 