using UnityEngine;

[CreateAssetMenu(fileName = "VaseType", menuName = "Scriptable Objects/VaseType")]
public class VaseType : ObstacleType
{   
    public override bool TakeDamage(int amount, bool isRocket, GridItem item)
    {
        // Get the ObstacleData component
        ObstacleData data = GetOrCreateObstacleData(item);
        
        // Handle the rule that vase takes only one damage per blast group or rocket
        if (isRocket)
        {
            // The vase takes one damage from each separate Rocket
            if (data.tookDamageFromRocket)
            {
                return false; // Already took damage from this rocket
            }
            data.tookDamageFromRocket = true;
        }
        else // Blast damage
        {
            if (data.tookDamageFromBlast)
            {
                return false; // Already took damage from this blast group
            }
            data.tookDamageFromBlast = true;
        }
        
        data.currentHealth -= amount;
        UpdateVisuals(item);
        
        return data.currentHealth <= 0;
    }
} 