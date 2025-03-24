using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using TMPro;
using System;
// Manages game levels including loading, transitions, and game state
public class LevelManager : Singleton<LevelManager>
{
    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameUIController gameUIController;
    [SerializeField] private ParticleSystem celebrationParticles;
    [SerializeField] private GameObject gameOverUI;

    [Header("Level Configuration")]
    [SerializeField] private TextAsset[] levelJsonFiles;
    [SerializeField] private int currentLevel = 0;
    #endregion

    #region Private Fields
    private GameGrid gameGrid;
    private GridItemPool itemPool;
    
    [SerializeField] private int remainingMoves;
    [SerializeField] private int remainingObstacles;
    private Dictionary<string, ObstacleCount> obstacleCounters = new Dictionary<string, ObstacleCount>();
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

    #region Initialization
    protected override void Init()
    {
        base.Init();
        InitializeObstacleCounters();
    }
    
    private void Start()
    {
        StartCoroutine(InitializeAndLoadLevel());
    }
    
    private IEnumerator InitializeAndLoadLevel()
    {
        yield return StartCoroutine(WaitForDependencies());
        
        ConnectGridEvents();
        LoadInitialLevel();
    }
    
    private IEnumerator WaitForDependencies()
    {
        // Wait for GameGrid to be available
        while (GameGrid.Instance == null)
        {
            Debug.LogWarning("Waiting for GameGrid to initialize");
            yield return null;
        }
        gameGrid = GameGrid.Instance as GameGrid;
        
        // Wait for ItemPool to be available
        while (GridItemPool.Instance == null)
        {
            Debug.LogWarning("Waiting for GridItemPool to initialize");
            yield return null;
        }
        itemPool = GridItemPool.Instance as GridItemPool;
    }
    
    private void LoadInitialLevel()
    {
        if (levelJsonFiles.Length > 0 && currentLevel < levelJsonFiles.Length)
        {
            LoadLevelFromJson(levelJsonFiles[currentLevel]);
        }
        else
        {
            Debug.LogWarning("No level files found!");
        }
    }
    
    private void InitializeObstacleCounters()
    {
        obstacleCounters.Add("bo", new ObstacleCount("Box"));
        obstacleCounters.Add("s", new ObstacleCount("Stone"));
        obstacleCounters.Add("v", new ObstacleCount("Vase"));
    }
    #endregion

    #region Event Management
    private new void OnDestroy()
    {
        DisconnectGridEvents();
        base.OnDestroy();
    }
    
    private void ConnectGridEvents()
    {
        if (gameGrid != null)
        {
            // First disconnect to prevent duplicates
            DisconnectGridEvents();
            
            // Then connect
            gameGrid.onObstacleDestroyed += OnObstacleDestroyed;
            gameGrid.onMoveMade += OnMoveMade;
        }
    }
    
    private void DisconnectGridEvents()
    {
        if (gameGrid != null)
        {
            gameGrid.onObstacleDestroyed -= OnObstacleDestroyed;
            gameGrid.onMoveMade -= OnMoveMade;
        }
    }
    #endregion

    #region Level Loading
    // Loads a level from a JSON TextAsset
    public void LoadLevelFromJson(TextAsset jsonFile)
    {
        if (!ValidateLevelData(jsonFile)) return;
        
        // Set the state to loading to prevent other operations
        currentState = GameState.Loading;
        
        // Disconnect events temporarily during level load
        DisconnectGridEvents();
        
        try
        {
            LevelData levelData = ParseLevelData(jsonFile);
            ApplyLevelData(levelData);
            
            // Reset the state to playing when successfully loaded
            currentState = GameState.Playing;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading level: {e.Message}");
            currentState = GameState.Playing; // Reset state even in case of error
        }
        finally
        {
            // Reconnect events after level load
            ConnectGridEvents();
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
        // Clean up existing grid
        CleanupGrid();
        
        // Initialize grid with new dimensions
        Vector2Int gridDimensions = new Vector2Int(levelData.grid_width, levelData.grid_height);
        gameGrid.InitializeGrid(gridDimensions);
        
        // Reset game state
        ResetGameState(levelData);
        
        // Place items on grid
        PlaceGridItems(levelData);
        
        // Update UI
        gameUIController.UpdateMoveCountUI(remainingMoves);
        gameUIController.UpdateGoalUI(obstacleCounters);
        
        Debug.Log($"Level {levelData.level_number} loaded. Obstacles: {remainingObstacles}, Moves: {remainingMoves}");
    }
    
    private void ResetGameState(LevelData levelData)
    {
        ResetObstacleCounts();
        remainingMoves = levelData.move_count;
    }
    #endregion

    #region Grid Management
    //TODO: Move all the grid management to the GameGrid Class
    
    // Cleans up the grid before loading a new level
    private void CleanupGrid()
    {   
        // Disconnect item events
        DisconnectItemEvents();
        
        // Remove all items
        RemoveAllItems();
        
        // Clear the grid data structure
        gameGrid.ClearGrid();
        
        // Clean up animations and pooled objects
        CleanupAnimationsAndPool();
    }
    
    private void DisconnectItemEvents()
    {
        // Disable all grid item callbacks to prevent unexpected behavior
        for (int y = 0; y < gameGrid.GridDimensions.y; y++)
        {
            for (int x = 0; x < gameGrid.GridDimensions.x; x++)
            {
                if (!gameGrid.IsEmpty(x, y))
                {
                    GridItem item = gameGrid.GetItemAt(x, y);
                    if (item != null)
                    {
                        // Disconnect events before removing
                        item.itemClicked -= gameGrid.OnItemClicked;
                    }
                }
            }
        }
    }
    
    private void RemoveAllItems()
    {
        // Return all items to the pool
        for (int y = 0; y < gameGrid.GridDimensions.y; y++)
        {
            for (int x = 0; x < gameGrid.GridDimensions.x; x++)
            {
                if (!gameGrid.IsEmpty(x, y))
                {
                    GridItem item = gameGrid.GetItemAt(x, y);
                    if (item != null)
                    {
                        gameGrid.RemoveGameItem(item);
                    }
                }
            }
        }
    }
    
    private void CleanupAnimationsAndPool()
    {
        DOTween.KillAll(true);
        try
        {
            // Return any active objects to the pool
            itemPool.ReturnAllObjectsToPool();
            
            // Reinitialize the pool with a reasonable size
            int poolSize = 100; // A default reasonable size
            itemPool.PoolObjects(poolSize);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during pool cleanup: {e.Message}");
        }
    }
    #endregion

    #region Obstacle Management
    private void ResetObstacleCounts()
    {
        foreach (var counter in obstacleCounters.Values)
        {
            counter.Reset();
        }
    }
    
    private void UpdateTotalObstacleCount()
    {
        remainingObstacles = 0;
        foreach (var counter in obstacleCounters.Values)
        {
            remainingObstacles += counter.Count;
        }
    }
    
    public void OnObstacleDestroyed(string obstacleType)
    {
        if (obstacleCounters.TryGetValue(obstacleType, out ObstacleCount counter))
        {
            counter.Decrease();
            remainingObstacles--;
            
            // Check win condition
            CheckGameState();
            gameUIController.UpdateGoalUI(obstacleCounters);
        }
    }
    #endregion

    #region Level Item Placement
    private void PlaceGridItems(LevelData levelData)
    {
        if (levelData.grid.Length != levelData.grid_width * levelData.grid_height)
        {
            Debug.LogError("Grid data length doesn't match grid dimensions!");
            return;
        }
        
        for (int i = 0; i < levelData.grid.Length; i++)
        {
            // Calculate x,y from linear index
            // The grid starts from bottom left and goes horizontally
            int x = i % levelData.grid_width;
            int y = i / levelData.grid_width;
            
            string itemCode = levelData.grid[i];

            GridItem item = CreateItemFromCode(itemCode);
            if (item != null)
            {
                gameGrid.PlaceItemOnGrid(item, x, y);
            }
        }
        
        // Calculate total obstacles (for win condition)
        UpdateTotalObstacleCount();
        gameGrid.UpdateAllGroups();
    }
    
    // Creates a grid item based on the item code
    private GridItem CreateItemFromCode(string itemCode)
    {
        // Skip empty cells
        if (string.IsNullOrEmpty(itemCode)) return null;
        
        GridItem item = null;
        
        // Handle different item types based on code
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
            
            // Increment obstacle count if counter exists
            if (obstacleCounters.TryGetValue(itemCode, out ObstacleCount counter))
            {
                counter.Increase();
            }
        }
        else
        {
            Debug.LogWarning($"Unknown item code: {itemCode}");
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
    #endregion

    #region Game Flow
    public void OnMoveMade()
    {
        remainingMoves--;
        gameUIController.UpdateMoveCountUI(remainingMoves);
        
        // Check if game is over after move
        CheckGameState();
    }
    private void CheckGameState()
    {
        // Don't make state changes if we're already in a transition
        if (currentState != GameState.Playing)
            return;

        // Wait for all animations to complete before checking game state
        if (DOTween.PlayingTweens()?.Count > 0)
        {
            StartCoroutine(WaitAndCheckGameState());
            return;
        }

        // Check if all obstacles are cleared
        if (remainingObstacles <= 0) 
        {
            currentState = GameState.LevelCompleted;
            Debug.Log("Level Completed!");
            
            celebrationParticles.Play();
            StartCoroutine(WaitForAnimationsToComplete(LoadNextLevel));
        }
        // Check if out of moves
        else if (remainingMoves <= 0)
        {
            currentState = GameState.GameOver;
            Debug.Log("Game Over - Out of moves!");
            gameOverUI.SetActive(true);
        }
    }

    private IEnumerator WaitAndCheckGameState()
    {
        // Wait until all tweens are completed
        while (DOTween.PlayingTweens()?.Count > 0)
        {
            yield return null;
        }
        
        // Recheck the game state after all animations are done
        CheckGameState();
    }

    // Coroutine to wait until all animations are finished
    private IEnumerator WaitForAnimationsToComplete(Action callback)
    {
        // Wait until all tweens are completed
        while (DOTween.PlayingTweens()?.Count > 0 || celebrationParticles.isPlaying)
        {
            yield return null;
        }
        
        // Call the callback function
        callback?.Invoke();
    }
    #endregion

    #region Level Navigation
    public void LoadNextLevel()
    {
        currentLevel++;
        if (currentLevel < levelJsonFiles.Length)
        {
            LoadLevelFromJson(levelJsonFiles[currentLevel]);
        }
        else
        {
            Debug.Log("No more levels available!");
            // TODO: Show game complete UI
            currentLevel = levelJsonFiles.Length - 1; // Stay at last level
            currentState = GameState.Playing; // Reset the state to allow continuing
        }
    }
    
    public void RestartLevel()
    {
        gameOverUI.SetActive(false);
        LoadLevelFromJson(levelJsonFiles[currentLevel]);
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelJsonFiles.Length)
        {
            currentLevel = levelIndex;
            LoadLevelFromJson(levelJsonFiles[currentLevel]);
        }
        else
        {
            Debug.LogError($"Level index {levelIndex} is out of range!");
        }
    }
    #endregion

    #region Data Classes
    [System.Serializable]
    public class LevelData
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;
        public string[] grid;
    }
    
    public class ObstacleCount
    {
        public string Name { get; private set; }
        public int Count { get; private set; }
        
        public ObstacleCount(string name)
        {
            Name = name;
            Count = 0;
        }
        
        public void Increase()
        {
            Count++;
        }
        
        public void Decrease()
        {
            Count = Mathf.Max(0, Count - 1);
        }
        
        public void Reset()
        {
            Count = 0;
        }
    }
    #endregion
} 