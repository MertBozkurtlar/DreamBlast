using UnityEngine;

// Stores instance-specific data for obstacles
[RequireComponent(typeof(GridItem))]
public class ObstacleData : MonoBehaviour
{
    public int currentHealth;
    public bool tookDamageFromBlast = false;
    public bool tookDamageFromRocket = false;
    
    public void ResetBlastDamage()
    {
        tookDamageFromBlast = false;
    }
    
    public void ResetRocketDamage()
    {
        tookDamageFromRocket = false;
    }
} 