using UnityEngine;

[CreateAssetMenu(fileName = "StoneType", menuName = "Scriptable Objects/StoneType")]
public class StoneType : ObstacleType
{   
    public override bool TakeDamage(int amount, bool isRocket, GridItem item)
    {
        // Stone only takes damage from rockets
        if (isRocket)
        {
            ObstacleData data = GetOrCreateObstacleData(item);
            data.currentHealth -= amount;
            return data.currentHealth <= 0;
        }
        return false;
    }
} 