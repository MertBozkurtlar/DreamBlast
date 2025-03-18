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
        transform.localScale = new Vector3(gridScale, gridScale, 1);

        // Set the position so that the grid is centered in the camera
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
        // Populate the grid
        GridItem item;

        for(int y = 0; y < Dimensions.y; y++)
            for(int x = 0; x < Dimensions.x; x++)
            {
                item = itemPool.GetRandomItem();
                PutItemAt(item, x, y);

                yield return new WaitForSeconds(0.1f);
            }
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
        // Check for matches
        List<GridItem> matches = MatchCheck(item);
        yield return null;

        if (matches.Count >= matchCount)
        {   
            // TODO: Play a match animation
            // Remove the matches
            RemoveMatches(matches);
        }
        yield return null;
    }

    private List<GridItem> MatchCheck(GridItem item){
        Debug.Log("Checking Matches for item at position: " + item.GridPosition);

        // Use Breadth First Search to find all items that are connected to the clicked item
        // and have the same type.
        HashSet<GridItem> visited = new HashSet<GridItem>();
        Queue<GridItem> queue = new Queue<GridItem>();
        List<GridItem> matches = new List<GridItem>();

        // Add the clicked item to the queue and visited
        queue.Enqueue(item);
        visited.Add(item);
        matches.Add(item);

        while(queue.Count > 0)
        {
            GridItem currentItem = queue.Dequeue();
            Debug.Log("Checking item at position: " + currentItem.GridPosition);

            // Check all 4 directions
            CheckDirection(currentItem, queue, matches, visited, Vector2Int.up);
            CheckDirection(currentItem, queue, matches, visited, Vector2Int.down);
            CheckDirection(currentItem, queue, matches, visited, Vector2Int.left);
            CheckDirection(currentItem, queue, matches, visited, Vector2Int.right);
        }

        Debug.Log("Matches found: " + matches.Count);
        Debug.Log("Visited: " + visited.Count);
        return matches;
    }

    private void RemoveMatches(List<GridItem> matches){
        foreach(GridItem match in matches)
        {
            RemoveItemAt(match.GridPosition);
        }
    }   

    private void CheckDirection(GridItem item, Queue<GridItem> queue, List<GridItem> matches, HashSet<GridItem> visited, Vector2Int direction){
        Vector2Int position = item.GridPosition + direction;
        if(!BoundsCheck(position))
        {
            return;
        }   

        // Get the item in the direction    
        GridItem itemInDirection = GetItemAt(position);
        if(itemInDirection == null)
        {
            return;
        }
        if (visited.Contains(itemInDirection))
        {
            return;
        } 
        
        visited.Add(itemInDirection);

        // If the item is a match, add it to the queue
        if(itemInDirection.Type == item.Type)
        {
            queue.Enqueue(itemInDirection);
            matches.Add(itemInDirection);
        }
    }
    
}