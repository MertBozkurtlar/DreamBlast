using UnityEngine;

public class GridItemPool : ObjectPool<GridItem>
{
    [Header("Item Types")]
    [SerializeField] private CubeType[] cubeTypes;
    [SerializeField] private RocketType horizontalRocketType;
    [SerializeField] private RocketType verticalRocketType;
    [SerializeField] private BoxType boxType;
    [SerializeField] private StoneType stoneType;
    [SerializeField] private VaseType vaseType;

    public void RandomizeType(GridItem item)
    {
        item.SetType(cubeTypes[Random.Range(0, cubeTypes.Length)]);
    }

    public GridItem GetRandomItem() {
        GridItem item = GetPooledObject();
        RandomizeType(item);
        return item;
    }
    
    // Method to get a specific cube type item
    public GridItem GetCubeOfType(string code) {
        GridItem item = GetPooledObject();
        
        switch (code) {
            case "r": // Red
                item.SetType(cubeTypes[0]);
                break;
            case "g": // Green
                item.SetType(cubeTypes[1]);
                break;
            case "b": // Blue
                item.SetType(cubeTypes[2]);
                break;
            case "y": // Yellow
                item.SetType(cubeTypes[3]);
                break;
            case "rand": // Random
            default:
                RandomizeType(item);
                break;
        }
        
        return item;
    }
    
    // Method to get a power-up type item
    public GridItem GetPowerUp(string code) {
        GridItem item = GetPooledObject();
        
        switch (code) {
            case "hro": // Horizontal Rocket
                item.SetType(horizontalRocketType);
                break;
            case "vro": // Vertical Rocket
                item.SetType(verticalRocketType);
                break;
            default:
                item.SetType(horizontalRocketType);
                break;
        }
        
        return item;
    }
    
    // Method to get an obstacle type item
    public GridItem GetObstacle(string code) {
        GridItem item = GetPooledObject();
        
        switch (code) {
            case "bo": // Box
                item.SetType(boxType);
                break;
            case "s": // Stone
                item.SetType(stoneType);
                break;
            case "v": // Vase
                item.SetType(vaseType);
                break;
            default:
                Debug.LogWarning("Unknown obstacle code: " + code);
                RandomizeType(item);
                break;
        }
        
        return item;
    }
}