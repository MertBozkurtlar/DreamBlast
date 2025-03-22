using UnityEngine;

[CreateAssetMenu(fileName = "BoxType", menuName = "Scriptable Objects/BoxType")]
public class BoxType : ObstacleType
{   
    public override bool TakeDamage(int amount, bool isRocket, GridItem item)
    {
        // Box takes damage from both adjacent blasts and rockets
        ObstacleData data = GetOrCreateObstacleData(item);
        
        // Apply damage
        data.currentHealth = Mathf.Max(0, data.currentHealth - amount);
        
        // Return true if the obstacle is destroyed
        return data.currentHealth <= 0;
    }
} 