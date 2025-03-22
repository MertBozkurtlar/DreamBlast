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
    public string typeName;
    
    [Tooltip("Default sprite for this item type")]
    public Sprite defaultSprite;
    
    // Checks if the item is eligible for upgrade to a special item
    public virtual void CheckSpecialEligibility(GridItem item, int groupSize) {}
    
    // Called when this item is matched by the player
    public virtual void OnMatch(GridItem item, GameGrid grid) {}
}
