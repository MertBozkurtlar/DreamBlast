using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid : GridSystem<GridItem>
{
    public GridItemPool itemPool;
    [SerializeField]
    private int matchCount;
    [SerializeField]
    private Vector2 itemOffset;
    [SerializeField]
    private float paddingPercent;
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
        float gridWidth = Dimensions.x;
        float gridHeight = Dimensions.y;

        // Limiting factor is the scale of the grid, which is also the "size" of each item
        // since the original "width" of the items is 1.
        gridScale = Mathf.Min((cameraWidth * (1 - paddingPercent))/ gridWidth, (cameraHeight * (1 - paddingPercent))/ gridHeight);
        gridScale = Mathf.Min(gridScale, 1);
        // If there is no reason to scale down, I leave it at 1
        transform.localScale = new Vector3(gridScale, gridScale, 1);

        // Set the position so that the grid is centered
        transform.position = new Vector3(-(gridWidth/2) * gridScale, -(gridHeight/2) * gridScale, 0);
    }

    public override bool PutItemAt(GridItem item, int x, int y, bool allowOverwrite = false)
    {
        bool success = base.PutItemAt(item, x, y, allowOverwrite);
        if(success)
        {
            item.gameObject.transform.SetParent(transform);
            item.gameObject.transform.localScale = new Vector3(1, 1, 1);
            item.gameObject.name = "o";
            item.gameObject.SetActive(true);
            item.gameObject.GetComponent<SpriteRenderer>().sortingOrder = y;
            item.gameObject.transform.position = transform.position + new Vector3(gridScale/2 + x * itemOffset.x * gridScale, gridScale/2 + y * itemOffset.y * gridScale, 0);
            item.SetGridPosition(new Vector2Int(x, y));
            item.itemClicked += OnItemClicked;
        }
        return success;
    }

    public override GridItem RemoveItemAt(int x, int y)
    {
        GridItem item = base.RemoveItemAt(x, y);
        item.itemClicked -= OnItemClicked;
        itemPool.ReturnObjectToPool(item);
        return item;
    }

    public IEnumerator PopulateGrid()
    {     
        // TODO: I need to seperate the data from the render so I can run the UpdateAllGroups in parallel
        // Populate the grid
        GridItem item;

        for(int y = 0; y < Dimensions.y; y++)
            for(int x = 0; x < Dimensions.x; x++)
            {
                item = itemPool.GetRandomItem();
                PutItemAt(item, x, y);
                yield return 0.02f;
            }

        yield return null;
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


        StartCoroutine(Match(item));
    }

    private IEnumerator Match(GridItem item){
        List<GridItem> group = GetConnectedGroup(item);
        if (group.Count >= matchCount)
        {
            RemoveMatches(group);
            UpdateAllGroups();
        }
        yield return null;
    }


    private void RemoveMatches(List<GridItem> matches){
        foreach(GridItem match in matches)
        {
            RemoveItemAt(match.GridPosition);
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

        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
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