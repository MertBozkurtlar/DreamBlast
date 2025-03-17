using System.Collections;
using UnityEngine;

public class GameGrid : GridSystem<GridItem>
{
    public GridItemPool itemPool;

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
        transform.localScale = new Vector3(gridScale, gridScale, 1);

        // Set the position so that the grid is centered in the camera
        transform.position = new Vector3(-(gridWidth/2) * gridScale, -(gridHeight/2) * gridScale, 0);
    }

    public IEnumerator PopulateGrid()
    {     
        // Populate the grid
        GridItem item;

        for(int y = 0; y < Dimensions.y; y++)
            for(int x = 0; x < Dimensions.x; x++)
            {
                item = itemPool.GetRandomItem();
                item.gameObject.transform.SetParent(transform);
                item.gameObject.transform.localScale = new Vector3(1, 1, 1);
                item.gameObject.name = "o";
                item.gameObject.SetActive(true);
                item.gameObject.GetComponent<SpriteRenderer>().sortingOrder = y;
                // The position is going to be bit off on the y axis, since the cube sprite is not square.
                // But it is negligible.
                item.gameObject.transform.position = transform.position + new Vector3(gridScale/2 + x * itemOffset.x * gridScale, gridScale/2 + y * itemOffset.y * gridScale, 0);

                PutItemAt(item, x, y);

                yield return new WaitForSeconds(0.1f);
            }
    }
}