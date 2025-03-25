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
    [SerializeField] private float itemMoveDuration = 1f;
    
    private GridItemPool itemPool;
    private GameManager gameManager;
    private float gridScale;
    private Dictionary<GridItem, List<GridItem>> cachedGroups = new Dictionary<GridItem, List<GridItem>>();
    
    // Event handlers for obstacle destruction and move made
    public Action<string> onObstacleDestroyed;
    public Action onMoveMade;

    private bool isProcessingMatches = false;
    
    public bool IsProcessingMatches => isProcessingMatches;

    public void SetProcessingState(bool isProcessing)
    {
        isProcessingMatches = isProcessing;
    }

    private void Start()
    {
        StartCoroutine(WaitForDependencies());
    }

    private IEnumerator WaitForDependencies()
    {
        while (GridItemPool.Instance == null || GameManager.Instance == null)
        {
            yield return null;
        }
        itemPool = GridItemPool.Instance as GridItemPool;
        gameManager = GameManager.Instance as GameManager;
    }

    public override void InitializeGrid(Vector2Int dimensions)
    {
        base.InitializeGrid(dimensions);

        float cameraHeight = Camera.main.orthographicSize * 2;
        float cameraWidth = cameraHeight * Camera.main.aspect;
        float gridWidth = GridDimensions.x;
        float gridHeight = GridDimensions.y;

        gridScale = Mathf.Min((cameraWidth * (1 - paddingPercent))/ gridWidth, (cameraHeight * (1 - paddingPercent))/ gridHeight);
        gridScale = Mathf.Min(gridScale, 1);
        transform.localScale = new Vector3(gridScale, gridScale, 1);

        transform.position = new Vector3(-(gridWidth/2) * gridScale + gridOffset.x, -(gridHeight/2) * gridScale + gridOffset.y, 0);

        gridBackground.transform.localPosition = new Vector3(gridWidth/2, gridHeight/2, 0);
        gridBackground.size = new Vector2(gridWidth + 0.5f, gridHeight + 0.5f);
    }

    public Vector3 GridToWorldPosition(int x, int y)
    {
        return transform.position + new Vector3(gridScale/2 + x * itemOffset.x * gridScale, gridScale/2 + y * itemOffset.y * gridScale, 0);
    }

    public void MoveGameItem(GridItem item, int x, int y)
    {
        Vector3 targetPosition = GridToWorldPosition(x, y);
        item.gameObject.transform.DOKill();
        item.gameObject.GetComponent<SpriteRenderer>().sortingOrder = y;
        item.gameObject.transform.DOMove(targetPosition, itemMoveDuration)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                if (DOTween.PlayingTweens()?.Count <= 1)
                {
                    isProcessingMatches = false;
                }
            });
    }

    public GridItem RemoveGameItem(GridItem item)
    {
        if (item.IsObstacle())
        {
            string obstacleCode = item.Type.typeCode;
            onObstacleDestroyed?.Invoke(obstacleCode);
        }
        
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
        
        if (DOTween.PlayingTweens()?.Count == 0)
        {
            isProcessingMatches = false;
        }
    }

    public bool PlaceItemOnGrid(GridItem item, int x, int y, int offScreenOffset = 0)
    {
        if (PutItemAt(item, x, y))
        {
            item.gameObject.transform.SetParent(transform);
            item.gameObject.transform.localScale = new Vector3(1,1,1);
            item.SetGridPosition(new Vector2Int(x, y));
            
            item.itemClicked += OnItemClicked;
            
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
        if (item == null)
        {
            Debug.Log("No item at position: " + item.GridPosition);
            return;
        }

        if(gameManager.CurrentState != GameManager.GameState.Playing)
            return;
            
        if (isProcessingMatches || DOTween.PlayingTweens()?.Count > 0)
        {
            Debug.Log("Cannot make new matches while processing previous ones");
            return;
        }
        
        if (item.Type.itemType != ItemType.Obstacle)
        {
            isProcessingMatches = true;
        }
        
        item.Type.OnMatch(item, this);
    }
    
    public void CollapseItems()
    {
        for(int x = 0; x != GridDimensions.x; ++x)
        {
            int emptySpot = 0;
            for(int y = 0; y != GridDimensions.y; ++y)
            {
                if(!IsEmpty(x, y))
                {
                    GridItem item = GetItemAt(x, y);
                    
                    bool shouldNotFall = !item.CanFall() || IsOnTopOfNonFallableItem(x, y);
                    
                    if (!shouldNotFall)
                    {
                        if(y != emptySpot)
                        {
                            item.SetGridPosition(new Vector2Int(x, emptySpot));
                            MoveItemTo(x, y, x, emptySpot);
                            MoveGameItem(item, x, emptySpot);
                        }
                        emptySpot++;
                    }
                    else
                    {
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
        
        if (DOTween.PlayingTweens()?.Count == 0)
        {
            isProcessingMatches = false;
        }
    }
    
    public void ApplyBlastDamage(Vector2Int position, int damage)
    {
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
        
        ResetRocketDamageFlags();
    }

    private bool IsOnTopOfNonFallableItem(int x, int y)
    {
        if (y == 0) return false;
        
        GridItem itemBelow = GetItemAt(x, y - 1);
        return itemBelow != null && !itemBelow.CanFall();
    }

    private bool IsBelowNonFallableItem(int x, int y)
    {
        for (int aboveY = y + 1; aboveY < GridDimensions.y; aboveY++)
        {
            GridItem itemAbove = GetItemAt(x, aboveY);
            if (itemAbove != null && !itemAbove.CanFall())
            {
                return true;
            }
        }
        return false;
    }
}