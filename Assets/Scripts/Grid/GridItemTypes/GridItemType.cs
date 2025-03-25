using System;
using UnityEngine;

public enum ItemType
{
    Cube,
    PowerUp,
    Obstacle
}

public abstract class GridItemType : ScriptableObject
{
    public virtual ItemType itemType { get; }
    [Tooltip("Display name of this item type")]
    public string typeCode;
    
    [Tooltip("Default sprite for this item type")]
    public Sprite defaultSprite;
    
    // Checks if the item is eligible for upgrade to a special item
    public virtual void CheckSpecialEligibility(GridItem item, int groupSize) {}
    
    // Called when this item is matched by the player
    public virtual void OnMatch(GridItem item, GameGrid grid) {}
    
    // Called when this item is damaged (e.g., by rocket or other power-up)
    public virtual void OnDamaged(GridItem item, GameGrid grid, int damage) {}
    
    // Called when this item is destroyed (e.g., by rocket, blast damage, or match)
    public virtual void OnItemDestroyed(GridItem item, GameGrid grid) {}
}
