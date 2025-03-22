using UnityEngine;

// Base class for all obstacle types in the game
[System.Serializable]
public abstract class ObstacleType : GridItemType, IDamageable
{
    public override ItemType itemType { get { return ItemType.Obstacle; } }
    
    [SerializeField] 
    [Tooltip("Maximum health points of this obstacle")]
    protected int maxHealth = 1;
    
    [SerializeField]
    [Tooltip("Sprite to display when the obstacle is damaged but not destroyed")]
    protected Sprite damagedSprite;

    [SerializeField]
    [Tooltip("If true, this obstacle will fall when there's an empty space below")]
    protected bool canFall = false;
    
    public int MaxHealth => maxHealth;
    public int CurrentHealth => GetCurrentHealth();
    public bool CanFall => canFall;

    public virtual void Initialize(GridItem item)
    {
        // Get or create the obstacle data component
        ObstacleData data = GetOrCreateObstacleData(item);
        
        // Initialize health in the instance data
        data.currentHealth = maxHealth;
        
        // Reset visual state
        item.SetSprite(defaultSprite);
    }
    
    protected ObstacleData GetOrCreateObstacleData(GridItem item)
    {
        if (!item.TryGetComponent(out ObstacleData data))
        {
            data = item.gameObject.AddComponent<ObstacleData>();
        }
        return data;
    }
    
    public virtual bool TakeDamage(int amount, bool isRocket, GridItem item)
    {
        // Get the instance-specific data
        ObstacleData data = GetOrCreateObstacleData(item);
        
        // Apply damage
        data.currentHealth = Mathf.Max(0, data.currentHealth - amount);
        
        // Return true if the obstacle is destroyed
        return data.currentHealth <= 0;
    }
    

    public virtual void UpdateVisuals(GridItem item)
    {
        // Get the instance-specific data
        ObstacleData data = GetOrCreateObstacleData(item);
        
        // Update sprite based on damage state
        if (data.currentHealth < maxHealth && data.currentHealth > 0 && damagedSprite != null)
        {
            item.SetSprite(damagedSprite);
        }
    }
    
    protected int GetCurrentHealth(GridItem item = null)
    {
        if (item == null) return maxHealth;
        
        if (item.TryGetComponent(out ObstacleData data))
        {
            return data.currentHealth;
        }
        
        return maxHealth;
    }
} 