using UnityEngine;

// Base class for all obstacle types in the game
[System.Serializable]
public abstract class ObstacleType : GridItemType, IDamageable
{
    public override ItemType itemType { get { return ItemType.Obstacle; } }
    
    [Tooltip("Maximum health points of this obstacle")]
    public int maxHealth = 1;
    
    [Tooltip("Sprite to display when the obstacle is damaged but not destroyed")]
    public Sprite damagedSprite;

    [Tooltip("If true, this obstacle will fall when there's an empty space below")]
    public bool canFall = false;
    
    [Tooltip("Particle system to play when the obstacle is destroyed")]
    public ParticleSystem destroyParticles;

    [Tooltip("Material for destroy particles effect")]
    public Material destroyParticleMaterial;
    
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
        
        // Check if destroyed
        bool isDestroyed = data.currentHealth <= 0;
        
        // Return true if the obstacle is destroyed
        return isDestroyed;
    }
    
    public override void OnItemDestroyed(GridItem item, GameGrid grid)
    {
        // Play particles when obstacle is destroyed (by any means)
        PlayDestroyParticles(item.transform.position);
    }
    
    protected void PlayDestroyParticles(Vector3 position)
    {
        if (destroyParticles != null)
        {
            // Create position with z = -1
            Vector3 particlePosition = new Vector3(position.x, position.y, -1f);
            ParticleSystem particles = Object.Instantiate(destroyParticles, particlePosition, Quaternion.identity);
            particles.GetComponent<ParticleSystemRenderer>().material = destroyParticleMaterial;
            particles.Play();
            
            // Auto-destroy particle system after it finishes
            Object.Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        }
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