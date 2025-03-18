using System.Collections.Generic;
using UnityEngine;

public abstract class GridSystem<T> : Singleton<GridSystem<T>>
{
    protected T[,] gridData;

    private Vector2Int gridDimensions = new Vector2Int(1, 1);

    public Vector2Int Dimensions
    {
        get
        {
            return gridDimensions;
        }
    }

    private bool isReady;

    public bool IsReady
    {
        get
        {
            return isReady;
        }
    }

    public virtual void InitializeGrid(Vector2Int dimensions)
    {
        if(dimensions.x < 1 || dimensions.y < 1)
            Debug.LogError("Grid dimensions cannot be less than 1.");

        this.gridDimensions = dimensions;

        gridData = new T[dimensions.x, dimensions.y];

        isReady = true;
    }

    public void ClearGrid()
    {
        gridData = new T[gridDimensions.x, gridDimensions.y];
    }

    public bool BoundsCheck(int x, int y)
    {
        if(!isReady)
            Debug.LogError("Grid is not initialized.");

        return x >= 0 && x < gridDimensions.x && y >= 0 && y < gridDimensions.y;
    }
    public bool BoundsCheck(Vector2Int position)
    {
        return BoundsCheck(position.x, position.y);
    }

    public bool IsEmpty(int x, int y)
    {
        if(!BoundsCheck(x, y))
            Debug.LogError("(" + x + ", " + y + ") is not on the grid.");

        //  check if the item is empty
        return EqualityComparer<T>.Default.Equals(gridData[x, y], default(T));
;    }
    public bool IsEmpty(Vector2Int position)
    {
        return IsEmpty(position.x, position.y);
    }

    //  put an item on the grid
    public virtual bool PutItemAt(T item, int x, int y, bool allowOverwrite = false)
    {
        if(!BoundsCheck(x, y))
            Debug.LogError("(" + x + ", " + y + ") is not on the grid.");

        if(!allowOverwrite && !IsEmpty(x, y))
            return false;

        gridData[x, y] = item;
        return true;
    }
    public bool PutItemAt(T item, Vector2Int position, bool allowOverwrite = false)
    {
        return PutItemAt(item, position.x, position.y, allowOverwrite);
    }

    public T GetItemAt(int x, int y)
    {
        if(!BoundsCheck(x, y))
            Debug.LogError("(" + x + ", " + y + ") is not on the grid.");

        return gridData[x, y];
    }
    public T GetItemAt(Vector2Int position)
    {
        return GetItemAt(position.x, position.y);
    }

    //  remove an item from the grid, also return it in case we want it
    public virtual T RemoveItemAt(int x, int y)
    {
        if(!BoundsCheck(x, y))
            Debug.LogError("(" + x + ", " + y + ") is not on the grid.");

        T temp = gridData[x, y];
        gridData[x, y] = default(T);
        return temp;
    }
    public T RemoveItemAt(Vector2Int position)
    {
        return RemoveItemAt(position.x, position.y);
    }

    public bool MoveItemTo(int x1, int y1, int x2, int y2, bool allowOverwrite = false)
    {
        if(!BoundsCheck(x1, y1))
            Debug.LogError("(" + x1 + ", " + y1 + ") is not on the grid.");

        if(!BoundsCheck(x2, y2))
            Debug.LogError("(" + x2 + ", " + y2 + ") is not on the grid.");

        if(!allowOverwrite && !IsEmpty(x2, y2))
            return false;

        gridData[x2, y2] = RemoveItemAt(x1, y1);
        return true;
    }
    public bool MoveItemTo(Vector2Int position1, Vector2Int position2, bool allowOverwrite = false)
    {
        return MoveItemTo(position1.x, position1.y, position2.x, position2.y, allowOverwrite);
    }
}