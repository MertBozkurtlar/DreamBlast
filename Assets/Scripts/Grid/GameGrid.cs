using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
public class GameGrid : GridSystem<GridItem>
{
    
    [Header("Grid Settings")]
    [SerializeField] private int offScreenOffset;
    [SerializeField] private Vector2 itemOffset;
    [SerializeField] private Vector2 gridOffset;
    [SerializeField] private float paddingPercent;
    [SerializeField] private SpriteRenderer gridBackground;
    private GridItemPool itemPool;
    private float gridScale;
    private Dictionary<GridItem, List<GridItem>> cachedGroups = new Dictionary<GridItem, List<GridItem>>();
    
    // Event handlers for obstacle destruction and move made
    public Action<string> onObstacleDestroyed;
    public Action onMoveMade;

    private void Start()
    {
        itemPool = (GridItemPool) GridItemPool.Instance;
    }

    public override void InitializeGrid(Vector2Int dimensions){
        base.InitializeGrid(dimensions);

        // Set the scale so that the grid fits within the camera bounds with padding
        float cameraHeight = Camera.main.orthographicSize * 2;
        float cameraWidth = cameraHeight * Camera.main.aspect;
        float gridWidth = GridDimensions.x;
        float gridHeight = GridDimensions.y;

        // Limiting factor is the scale of the grid, which is also the "size" of each item
        // since the original "width" of the items is 1.
        gridScale = Mathf.Min((cameraWidth * (1 - paddingPercent))/ gridWidth, (cameraHeight * (1 - paddingPercent))/ gridHeight);
        gridScale = Mathf.Min(gridScale, 1);
        // If there is no reason to scale down, I leave it at 1
        transform.localScale = new Vector3(gridScale, gridScale, 1);

        // Set the position so that the grid is centered
        transform.position = new Vector3(-(gridWidth/2) * gridScale + gridOffset.x, -(gridHeight/2) * gridScale + gridOffset.y, 0);

        // Center the grid background
        gridBackground.transform.localPosition = new Vector3(gridWidth/2, gridHeight/2, 0);

        // Sliced sprite size
        gridBackground.size = new Vector2(gridWidth + 0.5f, gridHeight + 0.5f);
    }

    public Vector3 GridToWorldPosition(int x, int y){
        return transform.position + new Vector3(gridScale/2 + x * itemOffset.x * gridScale, gridScale/2 + y * itemOffset.y * gridScale, 0);
    }

    public void MoveGameItem(GridItem item, int x, int y){
        Vector3 targetPosition = GridToWorldPosition(x, y);
        item.gameObject.transform.DOKill();
        item.gameObject.GetComponent<SpriteRenderer>().sortingOrder = y;
        item.gameObject.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutBounce);
    }

    public GridItem RemoveGameItem(GridItem item)
    {
        if (item.IsObstacle())
        {
            string obstacleCode = "";
            if (item.Type is BoxType) obstacleCode = "bo";
            else if (item.Type is StoneType) obstacleCode = "s";
            else if (item.Type is VaseType) obstacleCode = "v";
            
            onObstacleDestroyed?.Invoke(obstacleCode);
        }
        
        // Call OnItemDestroyed method on the item's type before removing it
        if (item.Type != null)
        {
            item.Type.OnItemDestroyed(item, this);
        }
        
        RemoveItemAt(item.GridPosition);
        item.itemClicked -= OnItemClicked;
        itemPool.ReturnObjectToPool(item);
        return item;
    }

    public void PopulateGrid()
    {     
        GridItem item;

        for(int y = 0; y < GridDimensions.y; y++)
            for(int x = 0; x < GridDimensions.x; x++)
            {
                if(IsEmpty(x,y) && !IsBelowNonFallableItem(x, y)){
                    item = itemPool.GetRandomItem();
                    bool success = PlaceItemOnGrid(item, x, y, offScreenOffset);
                    if(success)
                    {
                        MoveGameItem(item, x, y);
                    }
                }
            }
        
        UpdateAllGroups();
    }

     // Places a grid item on the grid at the specified position
    public bool PlaceItemOnGrid(GridItem item, int x, int y, int offScreenOffset = 0)
    {
        // Put the item on the grid
        if (PutItemAt(item, x, y))
        {
            // Set up transform
            item.gameObject.transform.SetParent(transform);
            item.gameObject.transform.localScale = new Vector3(1,1,1);
            item.SetGridPosition(new Vector2Int(x, y));
            
            // Connect events
            item.itemClicked += OnItemClicked;
            
            // Position item on the grid
            Vector3 targetPosition = GridToWorldPosition(x, y);
            item.gameObject.transform.position = targetPosition + new Vector3(0, offScreenOffset, 0);
            item.gameObject.SetActive(true);
            item.gameObject.GetComponent<SpriteRenderer>().sortingOrder = y;
            return true;
        }
        else
        {
            Debug.LogError($"Failed to place item at position ({x}, {y})");
            itemPool.ReturnObjectToPool(item);
            return false;
        }
    }

    public void OnItemClicked(GridItem item)
    {
        Debug.Log("Item clicked: " + item.GridPosition);

        if (item == null)
        {
            Debug.Log("No item at position: " + item.GridPosition);
            return;
        }

        if(LevelManager.Instance.CurrentState != LevelManager.GameState.Playing)
            return;
        
        item.Type.OnMatch(item, this);
    }
    
    public void CollapseItems()
    {
        for(int x = 0; x != GridDimensions.x; ++x)
        {
            int emptySpot = 0;
            // Scan from bottom to top, keeping track of the lowest empty spot
            for(int y = 0; y != GridDimensions.y; ++y)
            {
                if(!IsEmpty(x, y))
                {
                    GridItem item = GetItemAt(x, y);
                    
                    // Check if this item should be prevented from falling
                    bool shouldNotFall = !item.CanFall() || IsOnTopOfNonFallableItem(x, y);
                    
                    // Only items that can fall should be moved
                    if (!shouldNotFall)
                    {
                        if(y != emptySpot)
                        {
                            // Move item down to the empty spot
                            item.SetGridPosition(new Vector2Int(x, emptySpot));
                            MoveItemTo(x, y, x, emptySpot);
                            MoveGameItem(item, x, emptySpot);
                        }
                        emptySpot++;
                    }
                    else
                    {
                        // If the item can't fall and there's an empty spot below,
                        // the empty spot should be after this item
                        if (y > emptySpot)
                        {
                            emptySpot = y + 1;
                        }
                        else
                        {
                            emptySpot++;
                        }
                    }
                }
            }
        }
    }
    
    // Add method to apply blast damage to obstacles in adjacent cells
    public void ApplyBlastDamage(Vector2Int position, int damage)
    {
        // Get adjacent positions (not diagonals)
        Vector2Int[] directions = new Vector2Int[] {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int adjacentPos = position + dir;
            if (BoundsCheck(adjacentPos))
            {
                GridItem adjacentItem = GetItemAt(adjacentPos);
                if (adjacentItem != null && adjacentItem.IsObstacle())
                {
                    if (adjacentItem.TakeDamage(damage, false))
                    {
                        RemoveGameItem(adjacentItem);
                    }
                }
            }
        }
    }
    
    // Method to reset blast damage flags for vases
    public void ResetBlastFlags()
    {
        for (int y = 0; y < GridDimensions.y; y++)
        {
            for (int x = 0; x < GridDimensions.x; x++)
            {
                if (!IsEmpty(x, y))
                {
                    GridItem item = GetItemAt(new Vector2Int(x, y));
                    ObstacleData data = item.GetComponent<ObstacleData>();
                    if (data != null)
                    {
                        data.ResetBlastDamage();
                    }
                }
            }
        }
    }

    /*
    * I have implemented a cache mechanism to cache all connected items with the same type.
    * This way I can share the functionality between the Matching and the Special Eligibility checks.
    *
    * It is kind of similar to an Union-Find, but I am still using a BFS and just caching the results.
    */
    public List<GridItem> GetConnectedGroup(GridItem startItem)
    {
        if (cachedGroups.ContainsKey(startItem))
            return cachedGroups[startItem];

        List<GridItem> group = new List<GridItem>();
        HashSet<GridItem> visited = new HashSet<GridItem>();
        Queue<GridItem> queue = new Queue<GridItem>();

        queue.Enqueue(startItem);
        visited.Add(startItem);
        group.Add(startItem);

        while (queue.Count > 0)
        {
            GridItem current = queue.Dequeue();
            foreach (Vector2Int dir in new [] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighborPos = current.GridPosition + dir;
                if (!BoundsCheck(neighborPos))
                    continue;

                GridItem neighbor = GetItemAt(neighborPos);
                if (neighbor != null && neighbor.Type == startItem.Type && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    group.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        cachedGroups[startItem] = group;
        return group;
    }

    public void UpdateAllGroups()
    {
        cachedGroups.Clear();
        HashSet<GridItem> processed = new HashSet<GridItem>();

        for (int y = 0; y < GridDimensions.y; y++)
        {
            for (int x = 0; x < GridDimensions.x; x++)
            {
                GridItem item = GetItemAt(new Vector2Int(x, y));
                if (item == null)
                    continue;

                if (processed.Contains(item))
                    continue;

                List<GridItem> group = GetConnectedGroup(item);
                processed.UnionWith(group);
                cachedGroups[item] = group;

                foreach (var member in group)
                {
                    member.CheckSpecialEligibility(group.Count);
                }
            }
        }
    }    

    // Method to reset rocket damage flags for vases
    public void ResetRocketDamageFlags()
    {
        for (int y = 0; y < GridDimensions.y; y++)
        {
            for (int x = 0; x < GridDimensions.x; x++)
            {
                if (!IsEmpty(x, y))
                {
                    GridItem item = GetItemAt(new Vector2Int(x, y));
                    ObstacleData data = item.GetComponent<ObstacleData>();
                    if (data != null)
                    {
                        data.ResetRocketDamage();
                    }
                }
            }
        }
    }

    // Add method to apply rocket damage to obstacles in its path
    public void ApplyRocketDamage(List<Vector2Int> positions, int damage)
    {
        foreach (Vector2Int pos in positions)
        {
            if (BoundsCheck(pos))
            {
                GridItem item = GetItemAt(pos);
                if (item != null && item.IsObstacle())
                {
                    if (item.TakeDamage(damage, true))
                    {
                        RemoveGameItem(item);
                    }
                }
            }
        }
        
        // Reset rocket damage flags after all damage is applied
        ResetRocketDamageFlags();
    }

    // Helper method to check if an item is positioned directly above a non-fallable item
    private bool IsOnTopOfNonFallableItem(int x, int y)
    {
        // If we're at the bottom row, there's nothing below
        if (y == 0) return false;
        
        // Check the cell directly below this one
        GridItem itemBelow = GetItemAt(x, y - 1);
        
        // If there's an item below and it can't fall, this item should also not fall
        return itemBelow != null && !itemBelow.CanFall();
    }

    // Helper method to check if a position is below a non-fallable item
    private bool IsBelowNonFallableItem(int x, int y)
    {
        // Check all positions above this one in the same column
        for (int aboveY = y + 1; aboveY < GridDimensions.y; aboveY++)
        {
            GridItem itemAbove = GetItemAt(x, aboveY);
            if (itemAbove != null && !itemAbove.CanFall())
            {
                return true; // Found a non-fallable item above this position
            }
        }
        return false; // No non-fallable items above this position
    }
}