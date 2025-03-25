using UnityEngine;

public class GridItemPool : ObjectPool<GridItem>
{
    [Header("Item Types")]
    [SerializeField] private CubeType[] cubeTypes;
    [SerializeField] private RocketType horizontalRocketType;
    [SerializeField] private RocketType verticalRocketType;
    [SerializeField] private ObstacleType[] obstacleTypes;

    public ObstacleType[] ObstacleTypes => obstacleTypes;

    public void RandomizeType(GridItem item)
    {
        item.SetType(cubeTypes[Random.Range(0, cubeTypes.Length)]);
    }

    public GridItem GetRandomItem() {
        GridItem item = GetPooledObject();
        RandomizeType(item);
        return item;
    }

    public GridItem GetItemOfType(string code) {
        if (IsColorCubeCode(code)) {
            return GetCubeOfType(code);
        }
        else if (IsPowerUpCode(code)) {
            return GetPowerUp(code);
        }
        else if (IsObstacleCode(code)) {
            return GetObstacle(code);
        }
        else {
            Debug.LogWarning("Unknown item type code: " + code);
            return GetRandomItem();
        }
    }
    
    public GridItem GetCubeOfType(string code) {
        GridItem item = GetPooledObject();
        
        if (code == "rand") {
            RandomizeType(item);
            return item;
        }
        
        for (int i = 0; i < cubeTypes.Length; i++) {
            if (cubeTypes[i].typeCode == code) {
                item.SetType(cubeTypes[i]);
                return item;
            }
        }
        
        // Fallback to random if no matching type is found
        Debug.LogWarning("Unknown cube type code: " + code + ". Using random type instead.");
        RandomizeType(item);
        return item;
    }
    
    public GridItem GetPowerUp(string code) {
        GridItem item = GetPooledObject();
        
        if (horizontalRocketType.typeCode == code) {
            item.SetType(horizontalRocketType);
        }
        else if (verticalRocketType.typeCode == code) {
            item.SetType(verticalRocketType);
        }
        else {
            Debug.LogWarning("Unknown power-up code: " + code + ". Using horizontal rocket instead.");
            item.SetType(horizontalRocketType);
        }
        
        return item;
    }
    
    public GridItem GetObstacle(string code) {
        GridItem item = GetPooledObject();
        
        foreach (var obstacle in obstacleTypes) {
            if (obstacle.typeCode == code) {
                item.SetType(obstacle);
                return item;
            }
        }
        
        Debug.LogWarning("Unknown obstacle code: " + code + ". Using random obstacle instead.");
        RandomizeType(item);
        return item;
    }

    public bool IsColorCubeCode(string code)
    {
        if (code == "rand") return true;
        
        foreach (var cubeType in cubeTypes)
        {
            if (cubeType.typeCode == code)
                return true;
        }
        return false;
    }
    
    public bool IsPowerUpCode(string code)
    {
        return horizontalRocketType.typeCode == code || verticalRocketType.typeCode == code;
    }
    
    public bool IsObstacleCode(string code)
    {
        foreach (var obstacle in obstacleTypes) {
            if (obstacle.typeCode == code) {
                return true;
            }
        }
        return false;
    }
}