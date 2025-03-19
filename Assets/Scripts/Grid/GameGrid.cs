using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class GameGrid : GridSystem<GridItem>
{
    private GridItemPool itemPool;
    [SerializeField] private int matchCount;
    [SerializeField] private int offScreenOffset;
    [SerializeField] private Vector2 itemOffset;
    [SerializeField] private float paddingPercent;
    private float gridScale;
    private Dictionary<GridItem, List<GridItem>> cachedGroups = new Dictionary<GridItem, List<GridItem>>();

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
        transform.position = new Vector3(-(gridWidth/2) * gridScale, -(gridHeight/2) * gridScale, 0);
    }

    public Vector3 GridToWorldPosition(int x, int y){
        return transform.position + new Vector3(gridScale/2 + x * itemOffset.x * gridScale, gridScale/2 + y * itemOffset.y * gridScale, 0);
    }

    public void MoveGameItem(GridItem item, int x, int y){
        Vector3 targetPosition = GridToWorldPosition(x, y);
        item.gameObject.GetComponent<SpriteRenderer>().sortingOrder = y;
        item.gameObject.transform.DOKill();
        item.gameObject.transform.DOMove(targetPosition, 1f).SetEase(Ease.OutBounce);
    }

    public GridItem RemoveGameItem(GridItem item)
    {
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
                if(IsEmpty(x,y)){
                    item = itemPool.GetRandomItem();
                    bool success = PutItemAt(item, x, y);
                    if(success)
                    {
                        item.gameObject.transform.SetParent(transform);
                        item.gameObject.transform.localScale = new Vector3(1, 1, 1);
                        item.SetGridPosition(new Vector2Int(x, y));
                        item.itemClicked += OnItemClicked;

                        Vector3 targetPosition = GridToWorldPosition(x, y);
                        item.gameObject.transform.position = targetPosition + new Vector3(0, offScreenOffset, 0);
                        item.gameObject.SetActive(true);
                        MoveGameItem(item, x, y);
                    }
                }
            }
        
        UpdateAllGroups();
    }

    private void OnItemClicked(GridItem item)
    {
        Debug.Log("Item clicked: " + item.GridPosition);

        if(item == null)
        {
            Debug.Log("No item at position: " + item.GridPosition);
            return;
        }

        Match(item);
    }
    
    /*
    * Run the match logic for the given group of items
    */
    private void Match(GridItem item){
        List<GridItem> group = GetConnectedGroup(item);
        // TODO: If more than 4 items are matched, turn them into a rocket
        if (group.Count >= matchCount)
        {
            RemoveMatches(group);
            CollapseItems();
            PopulateGrid();
        }
        else {
            if (!DOTween.IsTweening(item.gameObject.transform))
                item.gameObject.transform.DOShakeRotation(0.2f, Vector3.forward * 15, 20, 1, false, ShakeRandomnessMode.Harmonic);
        }
    }

    private void RemoveMatches(List<GridItem> matches){
        foreach(GridItem match in matches)
        {
            RemoveGameItem(match);
        }
    }

    private void CollapseItems()
    {
        for(int x = 0; x != GridDimensions.x; ++x)
        {
            int emptySpot = 0;
            // Scan from bottom to top, keeping track of the lowest empty spot
            for(int y = 0; y != GridDimensions.y; ++y)
            {
                if(!IsEmpty(x, y))
                {
                    if(y != emptySpot)
                    {
                        // Move item down to the empty spot
                        GridItem item = GetItemAt(x, y);
                        item.SetGridPosition(new Vector2Int(x, emptySpot));
                        MoveItemTo(x, y, x, emptySpot);
                        MoveGameItem(item, x, emptySpot);
                    }
                    emptySpot++;
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
                    member.checkSpecialEligibility(group.Count);
                }
            }
        }
    }    
}