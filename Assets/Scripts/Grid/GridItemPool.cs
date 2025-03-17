using UnityEngine;

public class GridItemPool : ObjectPool<GridItem>
{
    [SerializeField]
    private CubeType[] cubeTypes;

    public void RandomizeType(GridItem item)
    {
        item.SetType(cubeTypes[Random.Range(0, cubeTypes.Length)]);
    }

    public GridItem GetRandomItem() {
        GridItem item = GetPooledObject();
        RandomizeType(item);
        return item;
    }
}