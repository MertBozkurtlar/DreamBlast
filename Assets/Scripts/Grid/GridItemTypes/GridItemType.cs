using System;
using UnityEngine;

public enum ItemType
{
    Cube,
    PowerUp
}

public abstract class GridItemType : ScriptableObject
{
    public string typeName;
    public Sprite defaultSprite;
    public virtual ItemType itemType { get; }
    
    public virtual void checkSpecialEligibility(GridItem item, int groupSize) {}
    
    public virtual void OnMatch(GridItem item, GameGrid grid) {}
}
