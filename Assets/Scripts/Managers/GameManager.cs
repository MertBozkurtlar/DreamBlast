using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Vector2Int gridDimensions;
    
    private GameGrid gameGrid;
    private GridItemPool itemPool;
    private void Start()
    {
        gameGrid = (GameGrid) GameGrid.Instance;
        itemPool = (GridItemPool) GridItemPool.Instance;

        // StartCoroutine(Setup());
    }

    private IEnumerator Setup() {
        // In the worst case, we will need two times of the grid size
        itemPool.PoolObjects(2 * gridDimensions.x * gridDimensions.y);
        gameGrid.InitializeGrid(gridDimensions);

        yield return null;

        gameGrid.PopulateGrid();
    }

    public void Reset()
    {
        gameGrid.ClearGrid();
        itemPool.ReturnAllObjectsToPool();
        gameGrid.PopulateGrid();
    }
}